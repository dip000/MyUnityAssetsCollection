using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*	NOTES:
 *		1.
 *		
 */

public class DrawerRebuild : MonoBehaviour
{
	Vector3 Q1;
	Vector3 P1;

	Vector3 pullVector = -Vector3.forward;

	public float pullDistance = 1;
	public Transform box;
	public Transform handle;

	[Header("Options")]
	public bool snapToEndPoints = true;
	public bool smartSnapping = true;
	public bool animateSnapping = false;
	public float snapSpeed = 10f;
	
	[Header("Advanced")]
	public float drawingTimeStep = 0.05f;
	public float snapTimeStep = 0.05f;

	[HideInInspector] public float pullVectorMagnitude;
	[HideInInspector] public bool isSwiping = false;
	[HideInInspector] public bool snapFinished = false;
	[HideInInspector] public Vector3 vectorResult = Vector3.zero;
	[HideInInspector] public Vector3 snappedTo = Vector3.zero;


	[HideInInspector] public Vector3 Q;

	Ray ray;	
	Plane slideSurface;
	Vector3 snapTowards = Vector3.zero;

	public void Start(){
		//Doesn't accept negative pulls (pushes)
		pullDistance = Mathf.Abs(pullDistance);
		
		pullVector *= pullDistance;
		pullVectorMagnitude = pullVector.magnitude;
		
		Q = pullVector.normalized;
		Q1 = box.position;
		
		slideSurface = new Plane(Vector3.up, box.position);
		LineDebuger(box.position, box.position + pullVector);
	}


	void Update()
    {
		//Conditions to start sliding the box
		if (Input.GetMouseButtonDown(0))
			if(HoverOverHandle())
				StartDraw();
	}

	//All-encompassing-foolproff way of detecting hover
	bool HoverOverHandle()
    {
		bool hasHitMe = RayCaster.HasHitMe( transform );
		return hasHitMe;
	}


	public void StartDraw(){

		//If the handler is being ckicked and drawn correctly, it starts the swipping
		//then stops on click release
		if(isSwiping == false){
			isSwiping = true;
			snapFinished = false;

			//Saves the first evaluation point
			P1 = GetMousePositionOverSlideSurface();

			StartCoroutine(DrawingMotion());
			DebugDrawer("DrawingMotion Started");
		}	
	}

    public void ResetDrawer()
    {
		StartCoroutine(SnapingMotion(Vector3.zero));
	}
	
	IEnumerator DrawingMotion(){
		
		//Break looping coroutine and resets on click release
		if(Input.GetMouseButton(0) == false){
			DebugDrawer("DrawingMotion stopped");
			isSwiping = false;

			if (snapToEndPoints)
			{
				float snapPoint = 0.5f;
				
				//If drawer was previusly closed, snaps open at 25% pull
				//If drawer was previusly open, snaps close at 25%  push
				if (smartSnapping)
				{
					if(snapTowards == Vector3.zero)
						snapPoint = 0.25f;
					else
						snapPoint = 0.75f;
				}
				
				//detects in which drag point was left, and snaps accordilngly
				if (vectorResult.magnitude < pullVector.magnitude * snapPoint)
				{
					snapTowards = Vector3.zero;
				}
				else
				{
					snapTowards = pullVector;
				}
				
				DebugDrawer("Snaping towards (local): " + snapTowards);
				DebugDrawer("Snaping point: " + snapPoint);
				StartCoroutine(SnapingMotion(snapTowards));
			}

			yield break;
		}

		//Start variables
		Vector3 P2;
		Vector3 P;
		Vector3 prjectionOntoQ;

		//Give a small delay to detect a substantial change of current point
		yield return new WaitForSeconds(drawingTimeStep);
		
		//save new point after delay to get the current swipe vector of mouse pointer P
		P2 = GetMousePositionOverSlideSurface();
		P = P2 - P1;
		
		//Q is the sliding direction. Both in pullVector coordenates (referenced to 0)
		prjectionOntoQ = Vector3.Project(P, Q);

		//Result is accumulative to have a better internal control of the output, like clamping it
		vectorResult += prjectionOntoQ;
		vectorResult = Vector3.Min(vectorResult, Vector3.zero);
		vectorResult = Vector3.Max(vectorResult, pullVector);
		
		//Returns to world coordenate of Q to apply projected point
		box.position = vectorResult + Q1;
		
		//Loops and resets
		StartCoroutine(DrawingMotion());
		P1 = P2;
		DebugDrawer("vectorResult=" + vectorResult + "; prjectionOntoQ=" + prjectionOntoQ + "; P=" + P + "; Q=" + Q);
	}
	

	IEnumerator SnapingMotion(Vector3 snapTowards)
	{
		float currentSnapValue = 0;

		while(currentSnapValue <= 1)
		{
			if(isSwiping == false)
			{
				currentSnapValue += snapSpeed * (snapTimeStep + Time.deltaTime);
				vectorResult = Vector3.Lerp(vectorResult, snapTowards, currentSnapValue);
				box.position = vectorResult + Q1;

				DebugDrawer("Currently Snapping at (local): " + vectorResult);
				yield return new WaitForSeconds(snapTimeStep);
			}
			else{
				snapFinished = true;
				snappedTo = vectorResult;
				break;
			}
		}
		
		if(animateSnapping){
			//Vector3 snapPosition = Vector3.Lerp(Vector3.zero, pullVector, 0.1f);
			//StartCoroutine(SnapingMotion(snappedTo*0.1f));
		}
	}
	
	Vector3 GetMousePositionOverSlideSurface(){
		//Update mouse position, casts a ray onto imaginary Plane, and returns the hit point
		float enter = 0;

		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		slideSurface.Raycast(ray, out enter);
		return ray.GetPoint(enter);
	}
	
	public bool showDebugs; void DebugDrawer(string text){ if(showDebugs) Debug.Log(text); }
    void LineDebuger(Vector3 _from, Vector3 _to){
        if (showDebugs)
            Debug.DrawRay(_from, _to, Color.red, 0.05f);
    }
}


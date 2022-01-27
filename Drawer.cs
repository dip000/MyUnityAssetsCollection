using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*	Use example for pushing/pulling a container 2 meters:
 *		
 *		
 */

public class Drawer : MonoBehaviour
{
	[Header("Setup")]
	public Vector3 pullVector = -Vector3.forward;
	public Transform container;
	public Transform handle;

	[Header("Options")]
	public bool snapToEndPoints = true;
	public bool smartSnapping = true;
	public float snapSpeed = 10f;
	
	[Header("Advanced")]
	public float drawingTimeStep = 0.01f;
	public float snapTimeStep = 0.01f;
	
	//Look up variables
	[HideInInspector] public float pullVectorMagnitude;
	[HideInInspector] public Vector3 pullVectorNormalized;
	[HideInInspector] public bool isSwiping = false;
	[HideInInspector] public bool snapFinished = false;
	[HideInInspector] public float snappedTo = 0;

	//Internal control
	Ray ray;	
	Plane slideSurface;
	Vector3 containerPosition;
	float snapTowards = 0;
	float dotResult;

	public void Start(){
		if(pullVector.y != 0){
			Debug.LogWarning("DRAWER. Drawer does not accept pulling to Y axis. Seted to 0");
			pullVector.y = 0;
		}
		
		pullVectorMagnitude = pullVector.magnitude;
		pullVectorNormalized = pullVector.normalized;
		containerPosition = container.position;

		slideSurface = new Plane(Vector3.up, containerPosition);
		LineDebuger(containerPosition, containerPosition + pullVector);
	}


	void Update()
    {
		//Conditions to start sliding the container
		if (Input.GetMouseButtonDown(0))
			if(HoverOverHandle())
				StartDraw();
	}

	//All-encompassing-foolproff way of detecting hover
	bool HoverOverHandle()
    {
		bool hasHitHandle = RayCaster.HasHit( handle );
		return hasHitHandle;
	}


	public void StartDraw(){

		//If the handler is being ckicked and drawn correctly, it starts the swipping
		//then stops on click release
		if(isSwiping == false){
			isSwiping = true;
			snapFinished = false;

			StartCoroutine(DrawingMotion());
			DebugDrawer("DrawingMotion Started");
		}	
	}

    public void ResetDrawer()
    {
		StartCoroutine(SnapingMotion(0));
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
					if(snapTowards == 0)
						snapPoint = 0.25f;
					else
						snapPoint = 0.75f;
				}
				
				//detects in which drag point was left, and snaps accordilngly
				if (dotResult < pullVectorMagnitude * snapPoint)
				{
					snapTowards = 0;
				}
				else
				{
					snapTowards = pullVectorMagnitude;
				}
				
				DebugDrawer("Snaping towards (local): " + snapTowards + "; Snaping point: " + snapPoint);
				StartCoroutine(SnapingMotion(snapTowards));
			}

			yield break;
		}
		
		Vector3 slide;
		
		yield return new WaitForSeconds(drawingTimeStep);
		
		//Localized current slide vector
		slide = GetMousePositionOverSlideSurface() - containerPosition;
		
		//Dot product is the magnitude of the proyection onto a normalized vector
		dotResult = Vector3.Dot(slide, pullVectorNormalized);
		dotResult = Mathf.Clamp(dotResult, 0, pullVectorMagnitude);
		DebugDrawer("dotResult:" + dotResult);
		
		//Directions dotResult to the pull vector and returns to world coordenate
		container.position = dotResult*pullVectorNormalized + containerPosition;

		//Loops and resets
		StartCoroutine(DrawingMotion());
	}
	

	IEnumerator SnapingMotion(float snapTowards)
	{
		float currentSnapValue = 0;

		while(currentSnapValue <= 1)
		{
			if(isSwiping == false)
			{
				currentSnapValue += snapSpeed * (snapTimeStep + Time.deltaTime);
				dotResult = Mathf.Lerp(dotResult, snapTowards, currentSnapValue);
				container.position = dotResult*pullVectorNormalized + containerPosition;

				DebugDrawer("Currently Snapping at (local): " + dotResult);
				yield return new WaitForSeconds(snapTimeStep);
			}
			else{
				snapFinished = true;
				snappedTo = dotResult;
				break;
			}
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


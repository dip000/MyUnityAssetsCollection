using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*	Use example for pushing/pulling a box 2 meters:
 *		
 *		
 */

public class DoorOpener : MonoBehaviour
{
	[Header("Setup")]
	public Transform handle;
	public Transform doorContainer;

	[Header("Options")]
	public float openingAngle = 120;
	public bool snapToEndPoints = true;
	public bool smartSnapping = true;
	public float snapSpeed = 10f;

	[Header("Advanced")]
	public float drawingTimeStep = 0.05f;
	public float snapTimeStep = 0.05f;

	[HideInInspector] public bool isSwiping = false;
	[HideInInspector] public bool snapFinished = false;

	Ray ray;	
	Plane slideSurface;
	
	float startPoint;
	float snapTowards;
	float openingAngleFixed;
	float snappedTo;
	float previousClamp;

	public void Start(){
		startPoint = doorContainer.localEulerAngles.y;
		snapTowards = startPoint;
		openingAngleFixed = (openingAngle<0) ? openingAngle+360 : openingAngle;
		previousClamp = startPoint;
		
		//Sliding surface is facing up, located at handle position
		slideSurface = new Plane(Vector3.up, handle.position);		
	}

	void Update()
    {
		//For surface plane debugging
		//box.position = GetMousePositionOverSlideSurface();
		
		//Conditions to start sliding the box
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

    /*public void ResetDrawer()
    {
		StartCoroutine(SnapingMotion(Vector3.zero));
	}*/
	float clampedAngle;
	bool breakDrawingMotion;
	IEnumerator DrawingMotion(){
		if(Input.GetMouseButton(0) == false){
			isSwiping = false;
			Snap();
			
			yield break;
		}
		
		yield return new WaitForSeconds(drawingTimeStep);
		
		Vector3 pointOverSlideSurface = GetMousePositionOverSlideSurface();		
		doorContainer.LookAt( pointOverSlideSurface );
		
		float angle = doorContainer.localEulerAngles.y;
		float fromAngle = startPoint;
		float toAngle = startPoint + openingAngleFixed;
		
		clampedAngle = ClampAngle(angle, fromAngle, toAngle);
		
		/*if(breakDrawingMotion){
			breakDrawingMotion = false;
			//isSwiping = false;
			yield break;
		}*/
			
		doorContainer.localEulerAngles = Vector3.up * clampedAngle;
		//doorContainer.localEulerAngles = Vector3.up * Mathf.Clamp(angle, startPoint, startPoint+openingAngle );
		
		DebugDrawer("DOOR OPENER. Angle:" + angle + "; clampedAngle:" + clampedAngle);
		StartCoroutine(DrawingMotion());
	}
	
	/*
	float ReformatAngle(float angle){
		return (angle + 180 + 360) % 360 - 180;
	}*/
	float ClampAngle( float angle, float fromAngle, float toAngle ){
		//Negative opening angle means closing. Thus, invertion
		if(openingAngle < 0){
			float switchReg = fromAngle;
			fromAngle = toAngle;
			toAngle = switchReg;
		}
		
		
		//Reformat to zero-to-negative instead of zero-to-360
		if(fromAngle > toAngle){
			if(angle > fromAngle){
				angle = angle - 360;
			}
			fromAngle = fromAngle - 360;
		}
		
		//If angle in range, return it
		if(angle < toAngle && angle > fromAngle){
			return angle;
		}
		
		//If not in range, clamp it
		else{
			snapFinished = false;
			float angleFixed = openingAngle<0?(fromAngle-toAngle) : (toAngle-fromAngle);
			DebugDrawer("DOOR OPENER. toAngle:" + toAngle + "; fromAngle:" + fromAngle + "; angleFixed:" + angleFixed);
			
			if(angle < (angleFixed*0.5f + 180)){
				/*if(previousClamp == fromAngle){
					isSwiping = false;
					breakDrawingMotion = true;
					DebugDrawer("DOOR OPENER. Snap " + fromAngle + " TO " + toAngle);
					StartCoroutine(SnapingMotion(fromAngle, toAngle));
					previousClamp = toAngle;
					return fromAngle;
				}
				previousClamp = toAngle;*/
				return fromAngle;
			}
			else{
				/*if(previousClamp == toAngle){
					isSwiping = false;
					breakDrawingMotion = true;
					DebugDrawer("DOOR OPENER. Snap " + fromAngle + " TO " + toAngle);
					StartCoroutine(SnapingMotion(toAngle, fromAngle));
					previousClamp = fromAngle;
					return toAngle;
				}
				previousClamp = fromAngle;*/
				return toAngle;
			}

		}
	}
	
	void Snap(){
		if (snapToEndPoints == false){
			return;
		}
		
		float snapPoint = 0.5f;
		
		//If drawer was previusly closed, snaps open at 25% pull
		//If drawer was previusly open, snaps close at 25%  push
		if (smartSnapping)
		{
			if(snapTowards == startPoint)
				snapPoint = 0.25f;
			else
				snapPoint = 0.75f;
		}
		
		//detects in which drag point was left, and snaps accordilngly
		float angleFix = openingAngleFixed<0 ? openingAngleFixed+360 : openingAngleFixed;
		if ((clampedAngle) > ((angleFix) * snapPoint))
		{
			snapTowards = startPoint;
		}
		else
		{
			snapTowards = openingAngle;
		}
		
		DebugDrawer("snapTowards:" + snapTowards + "; snapPoint:" + snapPoint);
		DebugDrawer("clampedAngle:" + clampedAngle + "; angleFix:"+angleFix);
		StartCoroutine(SnapingMotion(clampedAngle, snapTowards));
		//doorContainer.localEulerAngles = Vector3.up * snapTowards;

	}
	

	IEnumerator SnapingMotion(float angle, float snapTowards)
	{
		float currentSnapValue = 0;

		while(currentSnapValue <= 1)
		{
			if(isSwiping == false)
			{
				currentSnapValue += snapSpeed * (snapTimeStep + Time.deltaTime);
				angle = Mathf.Lerp(angle, snapTowards, currentSnapValue);
				doorContainer.localEulerAngles = Vector3.up * angle;

				DebugDrawer("Currently Snapping at (local): " + angle);
				yield return new WaitForSeconds(snapTimeStep);
			}
			else{
				clampedAngle = angle;
				snapFinished = true;
				snappedTo = angle;
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
	
    void OnDrawGizmosSelected()
    {
        if (showDebugs){
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(doorContainer.position, 5);
		}
    }
}


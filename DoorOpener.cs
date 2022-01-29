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
	
	float endPoint;
	float snapTowards;
	float openingAngleFixed;
	float snappedTo;
	float previousClamp;
	float distanceToOpen;
	bool isClosing = false;
	float clampedAngle;
	float backAngle;

	WaitForSeconds waitForDrawStep;
	WaitForSeconds waitForSnapStep;


	//BRANCH
	float startPoint;
	float currentDoorAngle;
	Vector3 currentDoorDirection;
	Vector3 rayDirection;
	float angleIncrement;
	float angleMin;
	float angleMax;

	public void Start(){
		
		//BRANCH
		angleIncrement = 0;
		currentDoorAngle = 0;
		currentDoorDirection = handle.position - doorContainer.position;
		//currentDoorDirection.y = 0;
		if(openingAngle > 0){
			angleMin = 0;
			angleMax = openingAngle;
		}
		else{
			angleMin = openingAngle;
			angleMax = 0;
		}
		Vector3 slideSurfaceDirection = Camera.main.transform.position - doorContainer.position;
		Debuger("angleMin:"+angleMin+"; angleMax:"+angleMax+"; slideSurfaceDirection:"+slideSurfaceDirection);
		Debug.DrawLine(doorContainer.position, doorContainer.position+slideSurfaceDirection, Color.red, 10f);
		
		//Module to avoid angles outside of range 0-360
		startPoint = ModuleAngle( doorContainer.eulerAngles.y );

		waitForDrawStep = new WaitForSeconds(drawingTimeStep);
		waitForSnapStep = new WaitForSeconds(snapTimeStep);
		
		//Sliding surface is facing up, located at handle position
		slideSurface = new Plane(slideSurfaceDirection, doorContainer.position);	
		Debuger("startPoint:"+startPoint+"; endPoint:"+endPoint+"; openingAngleFixed:"+openingAngleFixed+"; distanceToOpen:"+distanceToOpen);
		
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
			Debuger("DrawingMotion Started");
		}	
	}

    /*public void ResetDrawer()
    {
		StartCoroutine(SnapingMotion(Vector3.zero));
	}*/
	
	IEnumerator DrawingMotion(){
		if(Input.GetMouseButton(0) == false){
			isSwiping = false;
			Snap();
			
			yield break;
		}
		
		yield return waitForDrawStep;
		
		
		//BRANCH
		rayDirection = GetMousePositionOverSlideSurface() - doorContainer.position;
		//rayDirection.y = 0;
		angleIncrement += Vector3.SignedAngle(currentDoorDirection, rayDirection, Vector3.up);
		angleIncrement = Mathf.Clamp(angleIncrement, angleMin, angleMax);
		
		doorContainer.eulerAngles = Vector3.up * (angleIncrement + startPoint);
		
		Debuger("rayDirection:" + rayDirection + "; currentDoorDirection:" + currentDoorDirection + "; angleIncrement:"+angleIncrement);
		currentDoorDirection = rayDirection;
		
	
		StartCoroutine(DrawingMotion());
	}
	
	float AngleToPositive(float angle){
		return (angle<0) ? angle+360 : angle;
	}
	
	float ModuleAngle(float angle){
		return (angle + 360) % 360;
	}
	


	float ClampAngle( float angle, float fromAngle, float toAngle, float backAngle ){
		//Reformat to zero-to-negative instead of zero-to-360
		if(fromAngle > toAngle){
			if(angle > fromAngle){
				angle = angle - 360;
			}
			fromAngle = fromAngle - 360;
		}
		
		
		//If angle in range, return it
		if(angle < toAngle && angle > fromAngle){
			Debuger("DOOR OPENER. Inside Angle. angle:" + angle + "; fromAngle:" + fromAngle + "; toAngle:" + toAngle);
			return angle;
		}
		
		//TODO: consider zero cross on back angle
		//If not in range, clamp it
		else{
			Debuger("DOOR OPENER. Outside Angle. angle:" + angle + "; fromAngle:" + fromAngle + "; toAngle:" + toAngle);
			if( angle > backAngle ){
				return fromAngle;
			}
			else{
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
		if( angleIncrement > openingAngle * 0.5f )
		{
			snapTowards = endPoint;
		}
		else
		{
			snapTowards = startPoint;
		}
		//snapTowards = ClampAngle(clampedAngle, startPoint, endPoint, backAngle);;
		
		Debuger("snapTowards:" + snapTowards + "; snapPoint:" + snapPoint);
		Debuger("angleIncrement:" + angleIncrement + "; openingAngle:"+openingAngle);
		StartCoroutine(SnapingMotion(angleIncrement+startPoint, snapTowards));
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

				Debuger("Currently Snapping at (local): " + angle);
				yield return waitForSnapStep;
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
	
	public bool showDebugs; void Debuger(string text){ if(showDebugs) Debug.Log(text); }
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


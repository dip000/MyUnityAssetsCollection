using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*	Use example for pushing/pulling a box 2 meters:
 *		
 *		
 */

public class DoorOpener : MonoBehaviour
{
	[Header("Setup (Container must be pivoted)")]
	public Transform handle;
	public Transform doorContainer;
	public float doorHeight = 1.0f;

	[Header("Options")]
	[Tooltip("Negative to close. Also accepts several turns like 720")]
	public float openingAngle = 120;
	public bool snapToEndPoints = true;
	public bool smartSnapping = true;
	public float snapSpeed = 10f;

	[Header("Advanced")]
	[Tooltip("If door is in resting position, rotate it a little to let user know that can be interactued adding visual clarity and intuitiveness")]
	public float openingAngleOnClickDown = 0f;
	public float drawingTimeStep = 0.05f;
	public float snapTimeStep = 0.05f;

	//States
	[HideInInspector] public bool isSwiping = false;
	[HideInInspector] public bool snapFinished = false;

	//Internal control
	Ray ray;	
	Plane slideSurface;
	
	float angleIncrement;
	float snapTowards;
	float startPoint;
	float angleMin;
	float angleMax;

	Vector3 currentDoorDirection;
	Camera cam;
	
	WaitForSeconds waitForDrawStep;
	WaitForSeconds waitForSnapStep;

    private void Awake()
    {
		cam = Camera.main;
	}

	GameObject sphere;
	const float extraRadiusSpace = 0.3f;
	
    public void Start(){
		
		sphere = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		sphere.transform.position = doorContainer.position;
		float radius = (handle.position - doorContainer.position).magnitude;
		sphere.transform.localScale = new Vector3(sphere.transform.localScale.x*(radius*2 + extraRadiusSpace), doorHeight, sphere.transform.localScale.z*(radius*2 + 0.3f));
		sphere.GetComponent<Renderer>().enabled = false;
		
		sphere.AddComponent<MeshCollider>();
		Debuger("sphere radius:" + radius);
		
		
		//resting position of door is zero to keep track of direction and magnitude of rotation
		angleIncrement = 0;

		//if negative opening angle is a closing angle, switch min and max
		if(openingAngle < 0){
			angleMin = openingAngle;
			angleMax = 0;
		}
		else{
			angleMin = 0;
			angleMax = openingAngle;
		}

		//Module to avoid angles outside of range 0-360
		startPoint = ModuleAngle( doorContainer.eulerAngles.y );

		waitForDrawStep = new WaitForSeconds(drawingTimeStep);
		waitForSnapStep = new WaitForSeconds(snapTimeStep);

		Debuger("angleMin:" + angleMin + "; angleMax:" + angleMax);
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

		//Starts the swipping. Later stops on click release
		if (isSwiping == false){
			isSwiping = true;
			snapFinished = false;

			InitializeSlideSurface();
			OpenAngleOnClickDown();
			StartCoroutine(SwippingMotion());
			Debuger("SwippingMotion Started with angleIncrement:"+angleIncrement);
		}	
	}

	void InitializeSlideSurface()
    {
		//Sliding surface is pointing to camera and is located on click down position
		var planeSpawnPoint = GetPointOverHandleCollider();
		Vector3 slideSurfaceDirection = (cam.transform.position - planeSpawnPoint).normalized;
		currentDoorDirection = planeSpawnPoint - doorContainer.position;
		currentDoorDirection.y = 0;

		slideSurface = new Plane(slideSurfaceDirection, doorContainer.position);
		LineDebuger(cam.transform.position, planeSpawnPoint);
	}

	void OpenAngleOnClickDown()
    {
		//If there's even a angle to open
		if (openingAngleOnClickDown != 0)
		{
			//If door is in resting position, rotate it a little to let user know
			//that can be interactued adding visual clarity and intuitiveness
			if (angleIncrement == 0)
			{
				//sign of openingAngle is the opening direction
				angleIncrement += openingAngleOnClickDown * Mathf.Sign(openingAngle);
				ApplyRotationWithLocalAngle(angleIncrement);
			}
		}
	}


//--------------- ROTATION ACTIONS ----------------------------------------------

	IEnumerator SwippingMotion(){
		//Exits releasing the mouse button
		if(Input.GetMouseButton(0) == false){
			isSwiping = false;
			angleIncrement = Mathf.Clamp(angleIncrement, angleMin, angleMax);

			Snap();

			Debuger("SwippingMotion ended with angleIncrement:" + angleIncrement);
			yield break;
		}
		
		yield return waitForDrawStep;

		//Vector that points to the new direction to rotate
		Vector3 rayDirection = GetMousePositionOverSlideSurface() - doorContainer.position;
		rayDirection.y=0;

		//currentDoorDirection is a Vector that points to the direction before rotating
		//angleIncrement keeps track of all movements made to have a better internal control
		angleIncrement += Vector3.SignedAngle(currentDoorDirection, rayDirection, Vector3.up);
		float result = Mathf.Clamp(angleIncrement, angleMin, angleMax);
		ApplyRotationWithLocalAngle(result);
		
		currentDoorDirection = rayDirection;
		Debuger("angleIncrement:"+angleIncrement);
		StartCoroutine(SwippingMotion());
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
			if(snapTowards == 0)
				snapPoint = 0.25f;
			else
				snapPoint = 0.75f;
		}
		
		//detects in which drag point was left, and snaps accordilngly
		if( angleIncrement > openingAngle * snapPoint)
		{
			snapTowards = angleMax;
		}
		else
		{
			snapTowards = angleMin;
		}

		Debuger("angleIncrement:" + angleIncrement + "; openingAngle:"+openingAngle);
		Debuger("snapTowards:" + snapTowards + "; snapPoint:" + snapPoint);
		StartCoroutine(SnapingMotion(angleIncrement, snapTowards));
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
				ApplyRotationWithLocalAngle(angle);

				Debuger("Currently Snapping at (local): " + angle);
				yield return waitForSnapStep;
			}
			else{
				snapFinished = true;
				break;
			}
		}

		//Update internal angle
		angleIncrement = snapTowards;
		Debuger("SnappingMotion ended with angleIncrement:" + angleIncrement);
	}

	//-------------------- UTILITIES ----------------------------------------------
	Vector3 GetPointOverHandleCollider()
	{
        //Update mouse position, casts a ray onto imaginary Plane, and returns the hit point

        ray = cam.ScreenPointToRay( Input.mousePosition );
        handle.GetComponent<Collider>().Raycast(ray, out RaycastHit hit, 100);
		return hit.point;
	}

	void ApplyRotationWithLocalAngle(float localAngle)
	{
		doorContainer.eulerAngles = Vector3.up * (localAngle + startPoint);
	}

	/*public void ResetDrawer()
    {
		StartCoroutine(SnapingMotion(Vector3.zero));
	}*/

	float ModuleAngle(float angle)
	{
		return (angle + 360) % 360;
	}

	Vector3 GetMousePositionOverSlideSurface(){
		//Update mouse position, casts a ray onto imaginary Plane, and returns the hit point
		ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 point;

        if(sphere.transform.GetComponent<MeshCollider>().Raycast(ray, out RaycastHit hit, 100) == false){
            slideSurface.Raycast( ray, out float enter );
            point = ray.GetPoint(enter);
		}
		else{
			point = hit.point;
		}
		
		return point;
	}



	//------------------------ DEBUGGERS ----------------------------------------------------
	public bool showDebugs; void Debuger(string text){ if(showDebugs) Debug.Log(text); }
    void LineDebuger(Vector3 _from, Vector3 _to){
        if (showDebugs)
            Debug.DrawLine(_from, _to, Color.red, 10f);
    }
	
	
    void OnDrawGizmosSelected()
    {
        if (showDebugs){
			if(doorContainer != null && handle != null){
				float radius = (handle.position - doorContainer.position).magnitude + extraRadiusSpace;
				Vector3 halfHeight = Vector3.up*doorHeight;
				
				Gizmos.color = Color.green;
				Gizmos.DrawLine(Camera.main.transform.position, handle.position);
				Handles.color = Color.green;
				Handles.DrawWireArc(doorContainer.position, Vector3.up, Vector3.right, 360, radius);
				Handles.DrawWireArc(doorContainer.position + halfHeight, Vector3.up, Vector3.right, 360, radius);
				Handles.DrawWireArc(doorContainer.position - halfHeight, Vector3.up, Vector3.right, 360, radius);
			}
		}
    }
}


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
	public LayerMask handleLayerMask;
	public LayerMask cilinderLayerMask;

	[Header("Options")]
	public float openingAngle = 120;
	public bool snapToEndPoints = true;
	public bool smartSnapping = true;
	public float snapSpeed = 10f;
	public float openingAngleOnClickDown = 0f;

	[Header("Advanced")]
	public float drawingTimeStep = 0.05f;
	public float snapTimeStep = 0.05f;

	[HideInInspector] public bool isSwiping = false;
	[HideInInspector] public bool snapFinished = false;

	Ray ray;	
	Plane slideSurface;
	
	float endPoint;
	float snapTowards;

	WaitForSeconds waitForDrawStep;
	WaitForSeconds waitForSnapStep;


	float startPoint;
	Vector3 currentDoorDirection;
	Vector3 rayDirection;
	float angleIncrement;
	float angleMin;
	float angleMax;
	Camera cam;

    private void Awake()
    {
		cam = Camera.main;
	}

	GameObject sphere;
	
    public void Start(){
		
		sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.position = doorContainer.position;
		float radius = (handle.position - doorContainer.position).magnitude;
		sphere.transform.localScale = sphere.transform.localScale * (radius*2 + 0.3f);
		sphere.layer = 12;
		sphere.GetComponent<Renderer>().enabled = false;
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

		//TESTS
		/*var planeSpawnPoint = handle.position;
		Vector3 slideSurfaceDirection = (cam.transform.position - planeSpawnPoint).normalized;
		currentDoorDirection = planeSpawnPoint - doorContainer.position;
		slideSurface = new Plane(slideSurfaceDirection, planeSpawnPoint);*/
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
		rayDirection = GetMousePositionOverSlideSurface() - doorContainer.position;
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
		RaycastHit hit;

		ray = cam.ScreenPointToRay(Input.mousePosition);
		Physics.Raycast(ray, out hit, 100, handleLayerMask);
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
		RaycastHit hit;
		Vector3 point;

		if(Physics.Raycast(ray, out hit, 100, cilinderLayerMask) == false){
			float enter;
			slideSurface.Raycast(ray, out enter);
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
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(doorContainer.position, (handle.position - doorContainer.position).magnitude);
			Gizmos.DrawLine(Camera.main.transform.position, handle.position);
		}
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*  USE AS:

	void Start(){
		//Configure externally (optional)
		RayCaster.CastRaysSettings(RayCaster.Config.alwaysTimed, time:1f);
		RayCaster.CastRaysSettings(RayCaster.Config.asked);
		RayCaster.LayersSettings(distance:50f, detection:myLayerDetection, blocking:myLayerBlocker );
	}

    void Update()
    {
		//Hit a Component
        Transform targetComponent = RayCaster.GetFirstHitComponent<Transform>();
        bool hasHitComponent = (targetComponent == null) ? false : true;
		
		//Hit a Component ignoring layer blocker
        Transform targetComponent = RayCaster.GetFirstHitComponent<Transform>(false);
        bool hasHitComponent = (targetComponent == null) ? false : true;
		
		//Hit an object's transform
        bool hasHitMe = RayCaster.HasHit(transform);
    }

 *  NOTES:
 *      1.  
 *      2.  This scrip execution order is first than default time. If attribute doesn't work, do:
 *          Edit > ProjectSettings > ScriptExecutionOrder > Add > RayCaster (Place it above default time)
 */

//Executes before all other scripts
[DefaultExecutionOrder(-100)]
public class RayCaster : MonoBehaviour
{
	static public RayCaster instance;

	//Look up variables
    public static bool castRays = true;
    public static bool hasHitAnyObject = false;
    public static int numberOfObjectsHit = 0;
    public static RaycastHit[] objectsHit;

    //For runtime internal (not on inspector)
	static Config castRaysWhen;
    static LayerMask detectionLayerMask;
    static LayerMask dynamicBlockingMask;
    static float maxDistance;
	static float castRaysTime;
	static bool castWhenAsked = false;
	static bool castOnMouseClick = false;
	static bool castOnExternalCondition = false;
	
	//For runtime external
	public static bool castCondition = false;
	public enum Config {mouseClick, asked, always, alwaysTimed, externalCondition};

    //For Inspector set up (not on runtime)
    public LayerMask _detectionLayerMask = Physics.DefaultRaycastLayers;
    public LayerMask _dynamicBlockingMask = 0;
	public Config _castRaysWhen = Config.always;
	public float _castRaysTime = 0.05f;
    public float _maxDistance = 100;
    public bool _showDebugs = false;
	
	void Awake(){
		instance = this;
	}
	
    void Start()
    {		
		//Full setup. To change it, use: LayersSettings() and CastRaysSettings()
		//Must be in order: SetupValues, SeccurityStartChecks, CastRaysSetup
		SetupValues();
		SeccurityStartChecks();
		CastRaysSetup();
    }
	
	public static void LayersSettings(float distance, LayerMask detection, LayerMask blocking){
		//Setup
		instance._maxDistance = distance;
        instance._detectionLayerMask = detection;
        instance._dynamicBlockingMask = blocking;
		
		//Resets with previous CastRaysSettings
		instance.SetupValues();
		SeccurityStartChecks();
		CastRaysSetup();
		
		Debuger("RAYCASTER. Setted layer config. distance:" + distance + "; detectionMask:" + detection + "; blockingMask:" + blocking);
	}
	
	public static void CastRaysSettings(Config config ,float time=0){
		//Setup
		instance._castRaysWhen = config;
		if(config == Config.alwaysTimed){
			instance._castRaysTime = time;
		}
		
		//Resets with previous layersSettings
		instance.SetupValues();
		CastRaysSetup();
		
		Debuger("RAYCASTER. Setted ray casting config: " + config + "; Timed? " + (config == Config.alwaysTimed) + "; time:" + time);
	}

	void SetupValues(){
        //Set values. Unity not showing static variables in inspector :C
        showDebugs = _showDebugs;

        maxDistance = _maxDistance;
        detectionLayerMask = _detectionLayerMask;
        dynamicBlockingMask = _dynamicBlockingMask;
		castRaysWhen = _castRaysWhen;
		castRaysTime = _castRaysTime;
	}

	static void SeccurityStartChecks(){
        //Seccurity check 1. No mixed blocking mask
        float log2Mask = Mathf.Log(dynamicBlockingMask, 2);
        if( log2Mask != (int)log2Mask && dynamicBlockingMask !=0 )
        {
            Debug.LogWarning("RAYCASTER: dynamicBlockingMask cannot accept a mixed mask of " + dynamicBlockingMask + ". Reseted to 0");
            dynamicBlockingMask = 0;
        }

        //Seccurity check 2. Read layers must include the blocking mask
        // for a blocking to be detected
        detectionLayerMask |= dynamicBlockingMask;
	}
	
	static void CastRaysSetup(){
		//Resets
		instance.StopAllCoroutines();
		castWhenAsked = false;		
		castOnMouseClick = false;
		castOnExternalCondition = false;
		
		//Sets
		switch (castRaysWhen){
			case Config.alwaysTimed:
				if(castRaysTime == 0)
					Debug.LogWarning("RAYCASTER. Setted Timed Raycasting of time=0. Will act as always raycast");
				instance.StartCoroutine( instance.TimedCastRays() );
			break;
			
			case Config.always:
				castRaysTime = 0;
				instance.StartCoroutine( instance.TimedCastRays() );
			break;
			
			case Config.asked:
				castWhenAsked = true;
			break;
			
			case Config.mouseClick:
				castOnMouseClick = true;
			break;
			
			case Config.externalCondition:
				castOnExternalCondition = true;
			break;
		}
	}

    void Update()
    {
		if(castRays){
			if(castOnMouseClick && Input.GetMouseButtonDown(0)){
				CastRays();
				UpdateVariables();
			}
			else if(castOnExternalCondition && castCondition){
				CastRays();
				UpdateVariables();
			}
		}
    }
	
	IEnumerator TimedCastRays(){
		
		//Check global enable to continue or stop
		if (castRays)
        {
			CastRays();
			UpdateVariables();
        }
		else{
			yield break;
		}
		
		//Fixed delay between ray casts
		float delay = Mathf.Max(0, castRaysTime - Time.deltaTime);
		yield return new WaitForSeconds(delay);
		
		//loop raycasting
		StartCoroutine( TimedCastRays() );
	}

    static void CastRays()
    {
		//Ray from mouse pointer forwards
        Vector3 mouse = Input.mousePosition;
        Ray castPoint = Camera.main.ScreenPointToRay(mouse);

        //Cast a ray to everything and always sort by distance
        objectsHit = Physics.RaycastAll(castPoint, maxDistance, detectionLayerMask).OrderBy(h => h.distance).ToArray();
        RayDebuger(castPoint);
	}
	
	static void UpdateVariables(){
		numberOfObjectsHit = objectsHit.Length;
		hasHitAnyObject = numberOfObjectsHit > 0 ? true : false;

		for (int i=0; i< numberOfObjectsHit; i++)
		{
			Debuger("objectsHit[" + i + "] name:" + (objectsHit[i].transform.name));
		}
	}
	

//------------------ RUNTIME FUNCTIONS --------------------------------

    public static T GetFirstHitComponent<T>(bool layerBlocker=true)
    {
		CastWhenAskedCheck();
		
        T hitComponent = default(T);
        RaycastHit[] _objectsHit = objectsHit;

        for (int i = 0; i < numberOfObjectsHit; i++)
        {
            //Return found component if any
            T actualComponent = _objectsHit[i].transform.GetComponent<T>();
			bool componentFound = (actualComponent != null);
			
			//If current hit has a blocker layer, stop because 
			//there's no next valid hit component
			//Spare it if it is a blocker itself
			if ( layerBlocker && !componentFound ){
				if(DetectMaskBlocking(_objectsHit[i]))
					break;
			}
			
            if (componentFound)
            {
                hitComponent = actualComponent;
                break;
            }
        }

        return hitComponent;
    }

    static bool DetectMaskBlocking(RaycastHit hit)
    {
		//If there's an actual blocking layer
		if (dynamicBlockingMask <= 0)
		{
			return false;
		}

		//Cheap way of dectecting an object layer. Optimized but limited to 1 blocker layer
		if (Mathf.Pow(hit.transform.gameObject.layer, 2) != dynamicBlockingMask)
		{
			return false;
		}
		
		Debuger("Blocked mask: " + (int)dynamicBlockingMask + "; Of object " + hit.transform.name);
        return true;
    }


	public static bool HasHit(Transform target, bool layerBlocker=true){
		
		//Raycasts if castWhenAsked is false. Otherwise, use previous results 
		CastWhenAskedCheck();
		
		//Search through the objects hit to find target
        for (int i = 0; i < numberOfObjectsHit; i++)
        {
			bool isTarget = (target == objectsHit[i].transform);
			
			//If current hit has a blocker layer, stop because 
			//there's no next valid hit component.
			//Spare it if it is a blocker itself
			if( layerBlocker && !isTarget ){
				if ( DetectMaskBlocking(objectsHit[i]) ){
					return false;
				}
			}
			
			if (isTarget){
				return true;
			}
		}

		return false;
	}

	static void CastWhenAskedCheck(){
		if(castWhenAsked){
			CastRays();
			UpdateVariables();
		}
	}

    //NOT TESTED IN CURRENT VERSION
    public static T[] GetHitComponents<T>()
    {
		CastWhenAskedCheck();
		
        T[] hitComponents = null;
        int j=0;

        for (int i = 0; i < numberOfObjectsHit; i++)
        {
            T actualComponent = objectsHit[i].transform.GetComponent<T>();

            if (actualComponent != null)
            {
                hitComponents[j] = actualComponent;
                j++;
            }
        }

        return hitComponents;
    }

    // DEBUGGERS
    static bool showDebugs;
    static void Debuger(string text) { if (showDebugs) Debug.Log(text); }
    static void RayDebuger(Ray ray){
        if (showDebugs)
            Debug.DrawRay(ray.origin, ray.direction*maxDistance, Color.red, 0.05f);
    }
}

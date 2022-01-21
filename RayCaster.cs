using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*  USE AS:
    void Update()
    {
		//Hit a Component
        Transform targetComponent = RayCaster.GetFirstHitComponent<Transform>();
        bool hasHitTest = (targetComponent == null) ? false : true;
		
		//Hit me
        bool hasHitMe = RayCaster.HasHitMe(transform);
     }

 *  NOTES:
 *      1.  
 *      2.  This scrip execution order is first than default time:
 *          Edit > ProjectSettings > ScriptExecutionOrder > Add > RayCaster (Place it above default time)
 */

public class RayCaster : MonoBehaviour
{

    [TextArea]
    public string Notes = "Edit > ProjectSettings > ScriptExecutionOrder > Add > RayCaster (Place it above default time)";

    public static bool castRays = true;
    public static bool hasHitAnyObject = false;
    public static int numberOfObjectsHit = 0;

    public static RaycastHit[] objectsHit;

    //For runtime and by scritp (not on inspectr)
    static float maxDistance;
    static LayerMask detectionLayerMask;
    static LayerMask dynamicBlockingMask;

    //Depreciated
    //public static bool sortByDistance;

    //For Inspector set up (only on play, not runtime)
    public float _maxDistance = 100;
    public bool _sortByDistance = true;
    public LayerMask _detectionLayerMask = Physics.DefaultRaycastLayers;
    public LayerMask _dynamicBlockingMask = 0;
    public bool _showDebugs = false;

    static int blockingIndex;

    void Start()
    {
        //Set values. Unity not showing static variables in inspector :C
        showDebugs = _showDebugs;

        maxDistance = _maxDistance;
        detectionLayerMask = _detectionLayerMask;
        dynamicBlockingMask = _dynamicBlockingMask;

        //Seccurity check 1. No mixed blocking mask
        float log2Mask = Mathf.Log(dynamicBlockingMask, 2);
        if( log2Mask != (int)log2Mask && dynamicBlockingMask !=0 )
        {
            Debug.LogWarning("RAYCASTER: dynamicBlockingMask cannot accept a mixed mask. Reseted to 0");
            dynamicBlockingMask = 0;
        }

        //Seccurity check 2. Read layers must include the blocking mask
        // for a blocking to be detected
        detectionLayerMask |= dynamicBlockingMask;
    }

    void Update()
    {
        if (castRays)
        {
            CastRays();

            numberOfObjectsHit = objectsHit.Length;
            hasHitAnyObject = numberOfObjectsHit > 0 ? true : false;

            for (int i=0; i< numberOfObjectsHit; i++)
            {
                Debuger("objectsHit[" + i + "] name:" + (objectsHit[i].transform.name));
            }
        }

        //TESTS: PASS
        //Test targetComponent = GetFirstHitComponent<Test>(layerBlocker:false);
        //bool hasHitMe = (targetComponent != null) ? true : false;

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


    public static T GetFirstHitComponent<T>(bool layerBlocker=true)
    {
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


	public static bool HasHitMe(Transform me, bool layerBlocker=true){
		
        for (int i = 0; i < numberOfObjectsHit; i++)
        {				
			bool isMe = (me == objectsHit[i].transform);
			
			//If current hit has a blocker layer, stop because 
			//there's no next valid hit component.
			//Spare it if it is a blocker itself
			if( layerBlocker && !isMe ){
				if ( layerBlocker && DetectMaskBlocking(objectsHit[i]) ){
					return false;
				}
			}
			
			if (isMe){
				return true;
			}
		}

		return false;
	}


    //NOT TESTED IN CURRENT VERSION
    /*public static T[] GetHitComponents<T>()
    {
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
    }*/

    // DEBUGGERS
    public static bool showDebugs;
    static void Debuger(string text) { if (showDebugs) Debug.Log(text); }
    static void RayDebuger(Ray ray){
        if (showDebugs)
            Debug.DrawRay(ray.origin, ray.direction*maxDistance, Color.red, 0.05f);
    }
}

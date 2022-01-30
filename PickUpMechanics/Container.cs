using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{

	public Vector3 coordenates;

    [Header("DO NOT overlap Container's radiuses!")]
    public float detectionRadius = 1.0f;


    bool isOccupied = false;
    public Pickupable objectInside { get; private set; }
    [HideInInspector] public bool isBlocked;
	
	//Interesting bug if i declare this withouth private set..
	public bool finishedInitializing {get; private set;} = false;
	public bool yieldControlToExternal {get; private set;} = false;
	
	const bool occupied = true;
	const bool free = false;


    // Start is called before the first frame update
    void Start()
    {
		InitializeValues();
	}
	
	void InitializeValues(){
		objectInside = AutoFindItemNearby();
		
		if(objectInside == null){
			isOccupied = false;
		}
		else{
			isOccupied = true;
			objectInside.SetOccupancy( this );
			
			if(yieldControlToExternal == false){
				objectInside.transform.position = transform.position;
				objectInside.transform.parent = transform;
			}
		}
		
		finishedInitializing = true;
	}
	
	Pickupable AutoFindItemNearby(){
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);

        for (int i=0; i< hitColliders.Length; i++)
        {
            Pickupable testObjectInside = hitColliders[i].GetComponent<Pickupable>();
            if(testObjectInside != null)
            {
                Debuger("" + transform.name + " Has auto detected " + testObjectInside.transform.name);
                return testObjectInside;
            }
        }
		
		return null;
	}

    public void ResetOccupancy()
    {
		if(yieldControlToExternal){
			return;
		}

        isOccupied = false;
        objectInside = null;
    }
	
	public void SetOccupancy(Pickupable _objectInside){
		if(yieldControlToExternal){
			return;
		}
		
		isOccupied = true;
        objectInside = _objectInside;
	}

	
	public bool GetOccupancyState(){
		if(yieldControlToExternal){
			return free;
		}

		return isOccupied;
	}
	
	public void YieldControlToExternal(){
		yieldControlToExternal = true;
	}
	

    [Header("Check to visualize detection radius")]
    public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }

    void OnDrawGizmosSelected()
    {
        if (showDebugs)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}

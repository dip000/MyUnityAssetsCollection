using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{

	public Vector2 coordenates;

    bool isOccupied = false;
    public Pickupable objectInside { get; private set; }
    [HideInInspector] public bool isBlocked;
	
	//Interesting bug if i declare this withouth private set..
	public bool finishedInitializing {get; private set;} = false;
	public bool yieldControlToExternal {get; private set;} = false;
	
	const bool occupied = true;
	const bool free = false;


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
}

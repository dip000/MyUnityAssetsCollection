using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{

	public Vector2 coordenates;

    bool isOccupied = false;
	public Pickupable objectInside { get; private set; }
	public Transform containerRegister { get; private set; }
	[HideInInspector] public bool isBlocked = false;
	
	//Interesting bug if i declare this without private set..
	public bool finishedInitializing {get; private set;} = false;
	public bool yieldControlToExternal {get; private set;} = false;


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
		print( "_objectInside.myName:" + _objectInside.myName);
	}


	public void SetRegister(Transform _containerRegister){
		if(yieldControlToExternal){
			return;
		}
		
        containerRegister = _containerRegister;
		//print( "containerRegister.name:" + containerRegister.name );
	}

	
	public bool GetOccupancyState(){
		if(yieldControlToExternal){
			return PickUpMechanics.free;
		}

		return isOccupied;
	}
	
	public void YieldControlToExternal(){
		yieldControlToExternal = true;
	}

    public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }
}

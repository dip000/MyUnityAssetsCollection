using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* LISTEN TO EXTERNAL CONDITION:
 
    public bool PickupCondition { get { return (Random.value > 0.5f); } }
    void Awake(){ PickUpMechanics.ListenConditionalFrom(this, "PickupCondition"); }

*/

/*  NOTES:
 *      1. This script uses RayCaster.cs and have it somewhere in the scene
 *      2. 
 *      3. 
 */

public class PickUpMechanics : MonoBehaviour
{
    public Transform pickupHolder;

    public static bool hasObjectOnHand = false;
    public static Pickupable handObject;

    //Depreciating
	//    public static Transform targetTransform;

	public static Pickupable targetPickupable;
	public static Container targetContainer;

    public static event System.Action OnPickUp;
    public static event System.Action OnDrop;


    public const bool occupied = true;
    public const bool free = false;


    void Update()
    {
        
        if (Input.GetMouseButtonDown(0) == false)
        {
            return;
        }

        if (hasObjectOnHand)
        {
            Drop();
        }
        else
        {
            PickUp();
        }
    }


    void PickUp()
    {
        targetPickupable = RayCaster.GetFirstHitComponent<Pickupable>();

        if (targetPickupable == null)
        {
            Debuger("There's object to pick up");
            return;
        }

		Transform targetTransform = targetPickupable.transform;
        targetContainer = targetPickupable.myContainer;

        if (targetContainer.isBlocked)
        {
            Debuger("Container blocked");
            return;
        }

        if (GetExternalPickupCondition() == false)
        {
            Debuger("External condition to pick up was not met");
            return;
        }

        //Reset object and Reset its container
        targetContainer.ResetOccupancy();
        targetPickupable.ResetOccupancy();

        //Move object to hand and parent it
        hasObjectOnHand = true;
        handObject = targetPickupable;
		
        targetTransform.position = pickupHolder.position;
        targetTransform.parent = pickupHolder;

        //Publish event if there's anyone subscribed to it
        if(OnPickUp != null) OnPickUp();

        Debuger("Did Pick Up: " + targetTransform.name);
    }

    void Drop()
    {
        targetContainer = RayCaster.GetFirstHitComponent<Container>();
		
        if (targetContainer == null)
        {
            Debuger("There's no container");
            return;
        }

        Transform targetTransform = targetContainer.transform;
        targetPickupable = targetContainer.objectInside;

        if (targetContainer.isBlocked)
        {
            Debuger("Container blocked");
            return;
        }

        if (targetContainer.GetOccupancyState() == occupied)
        {
            Debuger("Container: " + targetTransform.name + " is full");
            return;
        }

        if (GetExternalDropCondition() == false)
        {
            Debuger("External condition to drop was not met");
            return;
        }


        //Set container and container's object
        targetContainer.SetOccupancy( handObject );
        handObject.SetOccupancy(targetContainer);
		handObject.AcomodateInMyContainer();

        hasObjectOnHand = false;

        //Publish event if there's anyone subscribed to it
        if (OnDrop != null) OnDrop();
        
        Debuger("Did Drop: " + targetTransform.name);
    }


    //------------  EXTERNAL ---------------------------------------------------
    //To listen to an external condition, leave null to ignore
    static List<System.Reflection.PropertyInfo> pickupCondition = new List<System.Reflection.PropertyInfo>();
    static List<object> pickupConditionInstance = new List<object>();
    static List<System.Reflection.PropertyInfo> dropCondition = new List<System.Reflection.PropertyInfo>();
    static List<object> dropConditionInstance = new List<object>();

    public static void ListenPickupConditionFrom(object _instance, string name)
    {
		
		System.Reflection.PropertyInfo condition = _instance?.GetType().GetProperty(name);
		
        if (condition == null)
        {
            Debug.LogWarning("PICKUP MECHANICS. Couldn't setup the listener to external conditional. Variable: " + name + " must exist in given instance, be public, be boolean, and have a getter to fetch a value");
        }
		else{
            print("Listener setup linked. Variable name: " + name);
			pickupConditionInstance.Add(_instance);
			pickupCondition.Add(condition);
		}
    }
	
    public static void ListenDropConditionFrom(object _instance, string name)
    {
		System.Reflection.PropertyInfo condition = _instance?.GetType().GetProperty(name);
		
        if (condition == null)
        {
            Debug.LogWarning("PICKUP MECHANICS. Couldn't setup the listener to external conditional. Variable: " + name + " must exist in given instance, be public, be boolean, and have a getter to fetch a value");
        }
		else{
            print("Listener setup linked. Variable name: " + name);
			dropConditionInstance.Add(_instance);
			dropCondition.Add(condition);
		}
    }

    bool GetExternalPickupCondition()
    {
        //By default, not having external condition returns true to continue
        bool result = true;
		for(var i=0; i<pickupCondition.Count; i++){
			if (pickupCondition[i] != null){
				result &= (bool)pickupCondition[i].GetValue(pickupConditionInstance[i], null);
				Debuger("Condition n:" + result);
			}
		}
        return result;
    }
	
    bool GetExternalDropCondition()
    {
        //By default, not having external condition returns true to continue
        bool result = true;
		for(var i=0; i<dropCondition.Count; i++){
			if (dropCondition[i] != null){
				result &= (bool)dropCondition[i].GetValue(dropConditionInstance[i], null);
				Debuger("Condition n:" + result);
			}
		}
        return result;
    }
    public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }
}

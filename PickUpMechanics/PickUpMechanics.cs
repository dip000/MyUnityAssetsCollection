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
    public float objectsToFillAContainer = 1;

    public static bool hasObjectOnHand = false;
    public static Transform handObject;

    public static event System.Action OnPickUp;
    public static event System.Action OnDrop;

    //To listen to an external condition, leave null to ignore
    static System.Reflection.PropertyInfo pickupCondition;
    static object pickupConditionInstance;
    static System.Reflection.PropertyInfo dropCondition;
    static object dropConditionInstance;

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
        Pickupable targetComponent = RayCaster.GetFirstHitComponent<Pickupable>();
        if (targetComponent == null)
        {
            Debuger("There's object to pick up");
            return;
        }

        if (targetComponent.myContainer.isBlocked)
        {
            Debuger("Container blocked");
            return;
        }

        if (GetExternalPickupCondition() == false)
        {
            Debuger("External condition to pick up was not met");
            return;
        }

        //Clear container's object (no practical use for this yet)
        targetComponent.myContainer.objectInside = null;

        //Empty object's fill and clear its container
        targetComponent.myContainer.fill--;
        targetComponent.myContainer = null;

        //Disable collider to avoid clicking it on your hand
        //targetComponent.transform.GetComponent<Collider>().enabled = false;

        //Move object to hand and parent it
        handObject = targetComponent.transform;
        targetComponent.transform.position = pickupHolder.position;
        targetComponent.transform.parent = pickupHolder;

        hasObjectOnHand = true;

        //Publish event if there's anyone subscribed to it
        if(OnPickUp != null) OnPickUp();

        Debuger("Did Pick Up: " + targetComponent.transform.name);
    }

    void Drop()
    {
        Container targetComponent = RayCaster.GetFirstHitComponent<Container>();
        if (targetComponent == null)
        {
            Debuger("There's no container");
            return;
        }

        if (targetComponent.isBlocked)
        {
            Debuger("Container blocked");
            return;
        }

        if (targetComponent.fill >= objectsToFillAContainer)
        {
            Debuger("Container: " + targetComponent.transform.name + " is full");
            return;
        }

        if (GetExternalDropCondition() == false)
        {
            Debuger("External condition to drop was not met");
            return;
        }

        //Set container's object (no practical use for this yet)
        targetComponent.objectInside = handObject.GetComponent<Pickupable>();

        //fill container and set object's new container
        targetComponent.fill++;
        handObject.GetComponent<Pickupable>().myContainer = targetComponent;

        //Enable object's collider to be able to click it again
        //handObject.GetComponent<Collider>().enabled = true;

        //Move object to container and parent it
        handObject.position = targetComponent.transform.position;
        handObject.parent = targetComponent.transform;
        handObject = null;

        hasObjectOnHand = false;

        //Publish event if there's anyone subscribed to it
        if (OnDrop != null) OnDrop();
        
        Debuger("Did Drop: " + targetComponent.transform.name);
    }


    public static void ListenPickupConditionFrom(object _instance, string name)
    {
        pickupConditionInstance = _instance;
        pickupCondition = pickupConditionInstance?.GetType().GetProperty(name);

        if (pickupCondition == null)
        {
            Debug.LogWarning("PICKUP MECHANICS. Couldn't setup the listener to external conditional. Variable: " + name + " must exist in given instance, be public, be boolean, and have a getter to fetch a value");
            pickupConditionInstance = null;
        }
    }
    public static void ListenDropConditionFrom(object _instance, string name)
    {
        dropConditionInstance = _instance;
        dropCondition = dropConditionInstance?.GetType().GetProperty(name);

        if (dropCondition == null)
        {
            Debug.LogWarning("PICKUP MECHANICS. Couldn't setup the listener to external conditional. Variable: " + name + " must exist in given instance, be public, be boolean, and have a getter to fetch a value");
            dropConditionInstance = null;
        }
    }

    bool GetExternalPickupCondition()
    {
        //By default, not having external condition returns true to continue
        bool result = true;
        if (pickupCondition != null)
            result = (bool)pickupCondition.GetValue(pickupConditionInstance, null);

        return result;
    }
    bool GetExternalDropCondition()
    {
        //By default, not having external condition returns true to continue
        bool result = true;
        if (dropCondition != null)
            result = (bool)dropCondition.GetValue(dropConditionInstance, null);

        return result;
    }
    public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }
}

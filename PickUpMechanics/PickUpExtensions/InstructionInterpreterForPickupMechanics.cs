using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InstructionInterpreterForPickupMechanics : MonoBehaviour
{
	Dictionary<Pickupable, Vector2> volumesCenter = new Dictionary<Pickupable, Vector2>();

	void Awake(){
		PickUpMechanics.OnPickUp += OnPickUp;
		PickUpMechanics.OnDrop += OnDrop;
	}

	//------------------------ REGISTERED EVENTS -------------------------------------
	void OnPickUp()
	{
		//Look up variables
		var item = PickUpMechanics.targetPickupable;
		var container = PickUpMechanics.targetContainer;

		//Get and update coordenates to register
		var globalCoordenates = Vector2Calculations.Globalize( item.shape, container.coordenates );

		//Calculate Volume Center if it is the first time this item was picked
		if( volumesCenter.ContainsKey( item ) == false )
			volumesCenter[item] = Vector2Calculations.RoundVector( Vector2Calculations.VolumeCenter( item.shape ) );
		
		//Router data to the corresponding register
		var register = PickUpMechanics.targetContainer.containerRegister;
		register.UpdateCoordenatesInOccupancyMap( globalCoordenates, PickUpMechanics.free );
	}

	void OnDrop(){
		//Look up variables
		Pickupable item = PickUpMechanics.targetPickupable;
		Container container = PickUpMechanics.targetContainer;
		
		//It places the item not from the left-down corner but from its center
		var placementPosition = container.coordenates - volumesCenter[item];

		//Get and update coordenates to register
		var globalCoordenates = Vector2Calculations.Globalize( item.shape, placementPosition );
		
		//Apply transforms and properties
        item.transform.position = ContainerCoordenatesToWorldSpacePosition(placementPosition);
        item.transform.parent = null;
	
		//Router data to the corresponding register
		var register = PickUpMechanics.targetContainer.containerRegister;
		var registerComponent = register.GetComponent<ArrayHolderRegister>();
		registerComponent.UpdateCoordenatesInOccupancyMap( globalCoordenates, PickUpMechanics.occupied );
	}
	
	Vector2 ContainerCoordenatesToWorldSpacePosition( Vector2 containerCoordenates)
    {
		var axis3D = new Vector3( containerCoordenates.x, 0, containerCoordenates.y );
		return axis3D + transform.position;
	}
	
	
}
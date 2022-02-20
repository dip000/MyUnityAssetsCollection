using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FridgeTypeConditionsForPickUpMechanics : MonoBehaviour
{
	public bool FridgeTypePickUpCondition { get => EvaluatePickUpCondition(); } 
	public bool FridgeTypeDropCondition { get => EvaluateDropCondition(); }
	

	void Awake(){
		//Register this script and its custom conditions as the aditional conditions to pick up
		PickUpMechanics.ListenPickupConditionFrom( this, "FridgeTypePickUpCondition");
		PickUpMechanics.ListenDropConditionFrom( this, "FridgeTypeDropCondition");
	}
	
	bool EvaluatePickUpCondition(){
		//Look up variables
		var register = PickUpMechanics.targetContainer.containerRegister;
		var occupancyMap = register.GetComponent<ArrayHolderRegister>().occupancyMap;
		var globalCoordinates = GetComponent<InstructionInterpreterForPickupMechanics>().CoordinatesOfTarget( PickUpMechanics.targetPickupable );

		// If item have at least one part in frontal row, pick up inconditionally
		for (int i=0; i< globalCoordinates.Length; i++){
			if( (int)globalCoordinates[i].y == 0 ){
				Debuger("Pick up condition (Custom Fridge-Type) of being in front row was a success");
				return true;
			}
		}

		//Must be at least one free slot in front of container (assuming target is at the back because of previous condition)
		for (int i = 0; i < globalCoordinates.Length; i++)
		{
			int x = (int)globalCoordinates[i].x;
			int y = (int)globalCoordinates[i].y - 1;
			if (occupancyMap[x, y] == PickUpMechanics.free)
			{
				Debuger("Pick up condition (Custom Fridge-Type) of have free a slot in frnt row was a success");
				return true;
			}
		}

		Debuger("Pick up condition did not met 'Custom Fridge-Type' conditions. See 'Custom Fridge-Type' rules");		
		return false;
	}
	
	
	bool EvaluateDropCondition(){
		//Look up variables
		var register = PickUpMechanics.targetContainer.containerRegister;
		var occupancyMap = register.GetComponent<ArrayHolderRegister>().occupancyMap;
		var globalCoordinates = GetComponent<InstructionInterpreterForPickupMechanics>().CoordinatesOfTarget( PickUpMechanics.handObject );

		for (int i = 0; i < globalCoordinates.Length; i++)
		{
			int x = (int)globalCoordinates[i].x;
			int y = (int)globalCoordinates[i].y;

			//If one coordenate is at the front, that's a pass
			if (y == 0)
			{
				return true;
			}

			//If it is at the back, it must have at least one free slot in front
			else if(occupancyMap[x, y-1] == PickUpMechanics.free)
			{
				Debuger("Drop condition (Custom Fridge-Type) of having a free slot in front row was a success");
				return true;
			}
		}

		Debuger("Drop condition did not met 'Custom Fridge-Type' conditions");
		return false;
	}

	
	public bool showDebugs=true; void Debuger(string text) { if (showDebugs) Debug.Log(text); }
}

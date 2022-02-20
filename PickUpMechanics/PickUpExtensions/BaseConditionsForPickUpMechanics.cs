using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseConditionsForPickUpMechanics : MonoBehaviour {
	
	public bool BasePickUpCondition { get => EvaluatePickUpCondition(); } 
	public bool BaseDropCondition { get => EvaluateDropCondition(); }

	void Awake(){
		//Register this script and its custom conditions as the aditional conditions to pick up
		PickUpMechanics.ListenPickupConditionFrom( this, "BasePickUpCondition");
		PickUpMechanics.ListenDropConditionFrom( this, "BaseDropCondition");
	}
	
	bool EvaluatePickUpCondition(){
		//Nothing extra to evaluate as base default
		return true;
	}
	
	bool EvaluateDropCondition(){

		if (EvaluateOccupancyConditions() == false){
			return false;
		}
		
		return true;
	}

	
	bool EvaluateOccupancyConditions(){
		//Look up variables
		var register = PickUpMechanics.targetContainer.containerRegister;
		var occupancyMap = register.GetComponent<ArrayHolderRegister>().occupancyMap;
		var globalCoordinates = GetComponent<InstructionInterpreterForPickupMechanics>().CoordinatesOfTarget( PickUpMechanics.handObject );
		
		for(int i=0; i<globalCoordinates.Length; i++){
			int x = (int) globalCoordinates[i].x;
			int y = (int) globalCoordinates[i].y;
			
			if( x > (occupancyMap.GetLength(0)-1) || y > (occupancyMap.GetLength(1)-1) ){
				Debuger("Item Drop couldn't happen because coordenate " + globalCoordinates[i] + " is outside of bounds (positive index)");
				return false;
			}
			if( x < 0 || y < 0 ){
				Debuger("Item Drop couldn't happen because coordenate " + globalCoordinates[i] + " is outside of bounds (negative index)");
				return false;
			}
			
			if( occupancyMap[x, y] == PickUpMechanics.occupied){
				Debuger("Item Drop couldn't happen because coordenate " + globalCoordinates[i] + " is occupied");
				return false;
			}
		}
		
		Debuger("Occupancy condition met");
		return true;
	}
	
	public bool showDebugs=true; void Debuger(string text) { if (showDebugs) Debug.Log(text); }

}
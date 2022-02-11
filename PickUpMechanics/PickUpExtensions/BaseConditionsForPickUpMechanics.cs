using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseConditionsForPickUpMechanics : MonoBehaviour {
	
	public bool BasePickUpCondition { get => EvaluatePickUpCondition(); } 
	public bool BaseDropCondition { get => EvaluateDropCondition(); }
	
	ArrayHolderRegister arrayHolderRegister;
	Pickupable item;
	Container container;
	bool[,] map;
	
	
	void Awake(){
		arrayHolderRegister = GetComponent<ArrayHolderRegister>();
		
		//Register this script and its custom conditions as the aditional conditions to pick up
		PickUpMechanics.ListenPickupConditionFrom( this, "BasePickUpCondition");
		PickUpMechanics.ListenDropConditionFrom( this, "BaseDropCondition");
	}
	
	bool EvaluatePickUpCondition(){
		//Nothing extra to evaluate as base default
		return true;
	}
	
	bool EvaluateDropCondition(){
		UpdateLookUpVariables();

		if (EvaluateOccupancyConditions() == false){
			return false;
		}
		
		return true;
	}

	
	bool EvaluateOccupancyConditions(){
		Vector2[] globalCoordenates = arrayHolderRegister.GetGlobalCoordenates();
		
		for(int i=0; i<globalCoordenates.Length; i++){
			int x = (int) globalCoordenates[i].x;
			int y = (int) globalCoordenates[i].y;
			
			if( x > (map.GetLength(0)-1) || y > (map.GetLength(1)-1) ){
				Debuger("Item Drop couldn't happen because coordenate " + globalCoordenates[i] + " is outside of bounds (positive index)");
				return false;
			}
			if( x < 0 || y < 0 ){
				Debuger("Item Drop couldn't happen because coordenate " + globalCoordenates[i] + " is outside of bounds (negative index)");
				return false;
			}
			
			if( map[x, y] == PickUpMechanics.occupied){
				Debuger("Item Drop couldn't happen because coordenate " + globalCoordenates[i] + " is occupied");
				return false;
			}
		}
		
		Debuger("Occupancy condition met");
		return true;
	}
	
	void UpdateLookUpVariables(){
		item = PickUpMechanics.targetPickupable;
		container = PickUpMechanics.targetContainer;
		map = ArrayHolderRegister.occupancyMap;
	}
	
	public bool showDebugs=true; void Debuger(string text) { if (showDebugs) Debug.Log(text); }

}
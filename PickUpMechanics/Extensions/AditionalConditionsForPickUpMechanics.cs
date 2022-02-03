using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AditionalConditionsForPickUpMechanics : MonoBehaviour {
	
	public bool PickUpCondition { get{ return EvaluatePickUpCondition(); } }
	
	public bool DropCondition { get{ return EvaluateDropCondition(); } }
	
	ArrayHolderRegister arrayHolderRegister;
	
	void Awake(){
		arrayHolderRegister = GetComponent<ArrayHolderRegister>();
		
		PickUpMechanics.ListenPickupConditionFrom( this, "PickUpCondition");
		PickUpMechanics.ListenDropConditionFrom( this, "DropCondition");
	}
	
	Pickupable item;
	Container container;
	bool[,] map;
	bool EvaluatePickUpCondition(){
		UpdateLookUpVariables();

		// Custom Fridge-Type Rules to pick u´p
		if ( EvaluateFridgePickUpConditions() == false){
			return false;
		}
		
		return true;
	}
	
	bool EvaluateFridgePickUpConditions(){
		Vector2[] coordenatesOfItem = item.coordenates;

		// If item have at least one part in frontal row, pick up inconditionally
		for (int i=0; i< coordenatesOfItem.Length; i++){
			if( (int)coordenatesOfItem[i].y == 0 ){
				Debuger("Pick up condition (Custom Fridge-Type) of being in front row was a success");
				return true;
			}
		}

		//Must be at least one free slot in front of container (assuming target is at the back because of previous condition)
		for (int i = 0; i < coordenatesOfItem.Length; i++)
		{
			int x = (int)coordenatesOfItem[i].x;
			int y = (int)coordenatesOfItem[i].y - 1;
			if (map[x, y] == PickUpMechanics.free)
			{
				Debuger("Pick up condition (Custom Fridge-Type) of have free a slot in frnt row was a success");
				return true;
			}
		}

		Debuger("Pick up did not met 'Custom Fridge-Type' conditions. See 'Custom Fridge-Type' rules");		
		return false;
	}
	
	bool EvaluateDropCondition(){
		UpdateLookUpVariables();

		/*// Custom Fridge-Type Rules to pick up
		if(EvaluateFridgeDropConditions() == false){
			return false;
		}*/

		if (EvaluateOccupancyConditions() == false){
			return false;
		}
		
		return true;
	}
	
	bool EvaluateFridgeDropConditions(){
		Vector2[] coordenatesOfItem = PickUpMechanics.handObject.coordenates;

		for (int i = 0; i < coordenatesOfItem.Length; i++)
		{
			//If coordenate is at the back
			if ((int)coordenatesOfItem[i].y == 1)
			{
				continue;
			}

			//Must be at least one free slot in front of container
			int x = (int)coordenatesOfItem[i].x;
			int y = (int)coordenatesOfItem[i].y - 1;
			if (map[x, y] == PickUpMechanics.free)
			{
				Debuger("Pick up condition (Custom Fridge-Type) of having a free slot in front row was a success");
				return true;
			}
		}

		Debuger("Pick up did not met 'Custom Fridge-Type' conditions. evaluating next sets of conditions..");
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
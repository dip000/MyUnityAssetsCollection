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
		
		if( EvaluateFridgePickUpConditions() ){
			return true;
		}
		
		return false;
	}
	
	bool EvaluateFridgePickUpConditions(){
		// If item have at least one part in frontal row, pick up inconditionally
		for(int i=0; i<item.coordenates.Length; i++){
			if( (int)item.coordenates[i].y == 0 ){
				Debuger("Pick up condition of being in front row was a success");
				return true;
			}
		}
		
		//Must be at least one free slot in front of container (assuming target is at the back because of previous condition)
		int x = (int)container.coordenates.x;
		int y = (int)container.coordenates.y-1;
		if( map[x, y] == PickUpMechanics.free){
			Debuger("Pick up condition of have free a slot in frnt row was a success");
			return true;
		}
		
		Debuger("Pick up was not in front nor had a free slot in front");		
		return false;
	}
	
	bool EvaluateDropCondition(){
		UpdateLookUpVariables();
		
		if(EvaluateFridgeDropConditions() == false){
			return false;
		}

		if(EvaluateOccupancyConditions() == false){
			return false;
		}
		
		return true;
	}
	
	bool EvaluateFridgeDropConditions(){
		//If target is at the back
		if((int)container.coordenates.y == 1){
			//Must be at least one free slot in front of container
			int x = (int)container.coordenates.x;
			int y = (int)container.coordenates.y-1;
			Debuger("Trying to Drop at the back and resulted: " + (map[x, y] == PickUpMechanics.free));
			return (map[x, y] == PickUpMechanics.free);
		}
		
		Debuger("Did not dropped at the back, evaluating next condition..");
		return true;
	}
	
	bool EvaluateOccupancyConditions(){
		Vector2[] globalCoordenates = arrayHolderRegister.GlobalizeCoordenates(arrayHolderRegister.localCoordenates, PickUpMechanics.targetContainer.coordenates);
		
		for(int i=0; i<globalCoordenates.Length; i++){
			int x = (int) globalCoordenates[i].x;
			int y = (int) globalCoordenates[i].y;
			
			if( x > (map.GetLength(0)-1) || y > (map.GetLength(1)-1) ){
				Debuger("Item Drop couldn't happen because coordenate " + globalCoordenates[i] + " is outside of bounds");
				return false;
			}
			
			if( map[x, y] != false ){
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
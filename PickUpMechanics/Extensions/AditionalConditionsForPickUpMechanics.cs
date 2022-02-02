using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AditionalConditionsForPickUpMechanics : MonoBehaviour {
	
	public bool PickUpCondition { get{ return EvaluatePickUpCondition(); } }
	
	public bool DropCondition { get{ return EvaluateDropCondition(); } }
	
	const bool free = false;
	const bool occupied = true;

	void Awake(){
		PickUpMechanics.ListenPickupConditionFrom( this, "PickUpCondition");

		PickUpMechanics.ListenDropConditionFrom( this, "DropCondition");
	}
	
	
	bool EvaluatePickUpCondition(){
		Pickupable target = PickUpMechanics.targetPickupable;
		Container container = target.myContainer;
		bool[,] map = ArrayHolderRegister.occupancyMap;
		//Container[] containers = ArrayHolderRegister.containers;
		
		// If item have at least one part in frontal row, pick up inconditionally
		for(int i=0; i<target.coordenates.Length; i++){
			if( (int)target.coordenates[i].y == 0 ){
				return true;
			}
		}
		
		//Must be at least one free slot in front of container (assuming target is at the back because of previous condition)
		int x = (int)container.coordenates.x;
		int y = (int)container.coordenates.y-1;
		if( map[x, y] == free){
			return true;
		}
		
		return false;
	}
	
	bool EvaluateDropCondition(){
		bool result = true;
		Container container = PickUpMechanics.targetContainer;
		Pickupable item = container.objectInside;
		bool[,] map = ArrayHolderRegister.occupancyMap;
		//Container[] containers = ArrayHolderRegister.containers;
		
		//If target is at the back
		if((int)container.coordenates.y == 1){
			//Must be at least one free slot in front of container
			int x = (int)container.coordenates.x;
			int y = (int)container.coordenates.y-1;
			result = (map[x, y] == free);
		}
		
		return result;
	}
	
	public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }

}
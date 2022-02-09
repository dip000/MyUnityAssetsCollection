using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FridgeTypeConditionsForPickUpMechanics : MonoBehaviour
{
	public bool FridgeTypePickUpCondition { get => EvaluatePickUpCondition(); } 
	public bool FridgeTypeDropCondition { get => EvaluateDropCondition(); }
	
	ArrayHolderRegister arrayHolderRegister;
	Pickupable item;
	Container container;
	bool[,] map;

	
	void Awake(){
		arrayHolderRegister = GetComponent<ArrayHolderRegister>();
		if(arrayHolderRegister == null)
			Debug.LogWarning("FRIDGE TYPE CONDITIONS. This component must be placed with ArrayHolderRegister");
		
		//Register this script and its custom conditions as the aditional conditions to pick up
		PickUpMechanics.ListenPickupConditionFrom( this, "FridgeTypePickUpCondition");
		PickUpMechanics.ListenDropConditionFrom( this, "FridgeTypeDropCondition");
	}
	
	bool EvaluatePickUpCondition(){
		UpdateLookUpVariables();

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

		arrayHolderRegister.UpdateDropPlacement();
		Vector2[] coordenatesOfItem = arrayHolderRegister.globalCoordenates;

		for (int i = 0; i < coordenatesOfItem.Length; i++)
		{
			int x = (int)coordenatesOfItem[i].x;
			int y = (int)coordenatesOfItem[i].y;

			print("(X,Y): "+x+","+y);
			
			//If one coordenate is at the front, that's a pass
			if (y == 0)
			{
				return true;
			}

			//If it is at the back, it must have at least one free slot in front
			else if(map[x, y-1] == PickUpMechanics.free)
			{
				Debuger("Pick up condition (Custom Fridge-Type) of having a free slot in front row was a success");
				return true;
			}
		}

		Debuger("Pick up did not met 'Custom Fridge-Type' conditions");
		return false;
	}


	
	void UpdateLookUpVariables(){
		item = PickUpMechanics.targetPickupable;
		container = PickUpMechanics.targetContainer;
		map = ArrayHolderRegister.occupancyMap;
	}
	
	
	public bool showDebugs=true; void Debuger(string text) { if (showDebugs) Debug.Log(text); }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AditionalConditionsForPickUpMechanics : MonoBehaviour {
	
	public bool PickUpCondition { get{ return EvaluatePickUpCondition(); } }
	
	public bool DropCondition { get{ return EvaluateDropCondition(); } }
	

	void Awake(){
		PickUpMechanics.ListenPickupConditionFrom( this, "PickUpCondition");

		PickUpMechanics.ListenDropConditionFrom( this, "DropCondition");
	}
	
	
	bool EvaluatePickUpCondition(){
		bool result = true;
		Pickupable target = PickUpMechanics.targetTransform.GetComponent<Pickupable>();
		bool[,] map = ArrayHolderRegister.occupancyMap;
		
		// If item have at least one part in frontal row, pick up inconditionally
		/*for(int i=0; i<coordenates.Length; i++){
			
		}*/
		
		
		return result;
	}
	
	bool EvaluateDropCondition(){
		return false;
	}
	
	public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }

}
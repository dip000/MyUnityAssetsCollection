using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ArrayHolderRegister : MonoBehaviour {

	public bool[,] occupancyMap;
	
	void Awake(){
		PickUpMechanics.OnPickUp += OnPickUp;
		PickUpMechanics.OnDrop += OnDrop;
	}


	public void Setup(Vector2 size)
    {
		occupancyMap = new bool[(int)size.x, (int)size.y];
		Debuger("Containers Setted up");
	}

	//------------------------ LIVE REGISTER -------------------------------------
	void OnDrop(){
		Pickupable item = PickUpMechanics.targetPickupable;
		Container container = PickUpMechanics.targetContainer;
		
		var placementPosition = volumesCenter[item] + container.coordenates;
		var globalCoordenates = Vector2Calculations.Globalize( item.shape, placementPosition );
		
		UpdateCoordenatesInOccupancyMap(globalCoordenates, PickUpMechanics.occupied);
		
        item.transform.position = /*ContainerCoordenatesToWorldSpacePosition*/(placementPosition);
        item.transform.parent = null;
		
	}
	
	void OnPickUp()
	{
		var item = PickUpMechanics.targetPickupable;
		var container = PickUpMechanics.targetContainer;
		var globalCoordenates = Vector2Calculations.Globalize(item.shape, container.coordenates);
		
		UpdateCoordenatesInOccupancyMap(globalCoordenates, PickUpMechanics.free);
		
		//Calculate Volume Center if it is the first of a kind item
		if( volumesCenter.ContainsKey( item ) == false ){
			volumesCenter[item] = Vector2Calculations.RoundVector( Vector2Calculations.VolumeCenter(item.shape) );
		}
	}

	
	Dictionary<Pickupable, Vector2> volumesCenter = new Dictionary<Pickupable, Vector2>();

	void UpdateCoordenatesInOccupancyMap(Vector2[] coordenates, bool state){
		
		try{
			for(int i=0; i<coordenates.Length; i++){
				occupancyMap[(int)coordenates[i].x, (int)coordenates[i].y] = state;
				Debuger("Updated["+i+"]coordenate in map: " + coordenates[i]);
			}
			
			Debuger("Updated " + coordenates.Length + " coordenates in occupancy map to the state: " + state);
		} catch{
			Debug.LogWarning("ARRAY HOLDER REGISTER. Can't register coordenate outside of bounds. ArrayHolderRegister.cs does not manage item Drop conditions. Refer to the script that does to know why it allowed this to happen, like AditionalConditionsForPickupMechnics.cs or the like");
		}
	}
	
    public bool showDebugs=true; void Debuger(string text) { if (showDebugs) Debug.Log(text); }
	void ArrayDebuger(Vector2[] vectorArray, string text="ArrayDebuger") { if (showDebugs) for (int i=0; i<vectorArray.Length; i++) Debug.Log(text + " ["+i+"] " + vectorArray[i]); }

}
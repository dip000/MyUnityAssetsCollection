using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ArrayHolderRegister : MonoBehaviour {

	public bool[,] occupancyMap;
	Dictionary<Pickupable, Vector2> volumesCenter = new Dictionary<Pickupable, Vector2>();

	void Awake(){
		PickUpMechanics.OnPickUp += OnPickUp;
		PickUpMechanics.OnDrop += OnDrop;
	}

	public void Setup(Vector2 size)
    {
		occupancyMap = new bool[(int)size.x, (int)size.y];
		Debuger("Containers Setted up");
	}

	//------------------------ REGISTER ON EVENTS -------------------------------------
	void OnPickUp()
	{
		//Look up variables
		var item = PickUpMechanics.targetPickupable;
		var container = PickUpMechanics.targetContainer;

		//Get and update coordenates to register
		var globalCoordenates = Vector2Calculations.Globalize( item.shape, container.coordenates );
		UpdateCoordenatesInOccupancyMap( globalCoordenates, PickUpMechanics.free );

		//Calculate Volume Center if it is the first time this item was picked
		if( volumesCenter.ContainsKey( item ) == false )
			volumesCenter[item] = Vector2Calculations.RoundVector( Vector2Calculations.VolumeCenter( item.shape ) );
	}

	void OnDrop(){
		//Look up variables
		Pickupable item = PickUpMechanics.targetPickupable;
		Container container = PickUpMechanics.targetContainer;
		
		//It places the item not from the left-down corner but from its center
		var placementPosition = container.coordenates - volumesCenter[item];

		//Get and update coordenates to register
		var globalCoordenates = Vector2Calculations.Globalize( item.shape, placementPosition );
		UpdateCoordenatesInOccupancyMap(globalCoordenates, PickUpMechanics.occupied);
		
		//Apply transforms and properties
        item.transform.position = ContainerCoordenatesToWorldSpacePosition(placementPosition);
        item.transform.parent = null;
	}

	Vector2 ContainerCoordenatesToWorldSpacePosition( Vector2 containerCoordenates)
    {
		var axis3D = new Vector3( containerCoordenates.x, 0, containerCoordenates.y );
		return axis3D + transform.position;
	}

	public void UpdateCoordenatesInOccupancyMap(Vector2[] coordenates, bool state){
		
		try{
			string debugString = "Updated "+ coordenates.Length + " coordenates in map: ";
			for(int i=0; i<coordenates.Length; i++){
				occupancyMap[(int)coordenates[i].x, (int)coordenates[i].y] = state;
				debugString += ", " + coordenates[i].ToString();
			}
			Debuger( debugString );
		} catch{
			Debug.LogWarning("ARRAY HOLDER REGISTER. Can't register coordenate outside of bounds. ArrayHolderRegister.cs does not manage item Drop conditions. Refer to the script that does to know why it allowed this to happen, like AditionalConditionsForPickupMechnics.cs or the like");
		}
	}

	//------------------------ DEBUGERS -------------------------------------
	public bool showDebugs=true; void Debuger(string text) { if (showDebugs) Debug.Log(text); }
	void ArrayDebuger(Vector2[] vectorArray, string text="ArrayDebuger") { if (showDebugs) for (int i=0; i<vectorArray.Length; i++) Debug.Log(text + " ["+i+"] " + vectorArray[i]); }

}
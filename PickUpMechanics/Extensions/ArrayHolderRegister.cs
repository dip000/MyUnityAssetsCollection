using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ArrayHolderRegister : MonoBehaviour {

	public static Container[,] containers;
	public static bool[,] occupancyMap;
	
	int xLength;
	int yLength;

	void Awake(){
		PickUpMechanics.OnPickUp += OnPickUp;
		PickUpMechanics.OnDrop += OnDrop;
	}

	public void RegisterContainers(Container[,] _containers)
    {
		if (_containers.Length <= 0)
		{
			Debug.LogWarning("ARRAY HOLDER REGISTER. No containers in scene to manage");
		}

		containers = _containers;
		occupancyMap = new bool[containers.GetLength(0), containers.GetLength(1)];
	}

	public void SetupContainers(){

		occupancyMap = new bool[containers.GetLength(0), containers.GetLength(1)];

		Debuger("Containers Setted up");
	}
	
	void OnPickUp(){
		Pickupable item = PickUpMechanics.targetPickupable;
		Container container = PickUpMechanics.targetContainer;

		Vector2[] globalCoordenates = item.coordenates;
		Vector2 indexCoordenates = container.coordenates;
		UpdateCoordenatesInOccupancyMap(globalCoordenates, PickUpMechanics.free);

		Vector2[] localCoordenates;
		localCoordenates = LocalizeCoordenates(globalCoordenates, indexCoordenates);
	}
	
	void OnDrop(){
		Pickupable item = PickUpMechanics.handObject.GetComponent<Pickupable>();
		Container container = PickUpMechanics.targetContainer;
		
		//Test
		Vector2[] coordenatesToUpdate = new Vector2[1];
		coordenatesToUpdate[0] = container.coordenates;
		//CoordenatesToObjectShape(coordenatesToUpdate);
		
		item.SetCoordenates( coordenatesToUpdate );
		UpdateCoordenatesInOccupancyMap(coordenatesToUpdate, PickUpMechanics.occupied);
		
		//Update object's coordenates

	}

	Vector2[] LocalizeCoordenates(Vector2[] globalCoordenates, Vector2 indexCoordenate)
    {
		Vector2[] localizedCoordenates = new Vector2[globalCoordenates.Length];

		for (int i=0; i< globalCoordenates.Length; i++)
        {
			localizedCoordenates[i] -= indexCoordenate;
		}

		return localizedCoordenates;

	}

	/*public void PlaceObjectInGrid(int i)
	{
		//Only by 90 degrees and max 3 times
		int rotationTimes = (int)(itemRotations[i] / 90) % 4;
		int rotationAngleClamp = rotationTimes * 90;

		//Rotate and Globalize coordenates
		Vector2 sum = Vector2.zero;
		Vector2[] localCoordenates = RotateMatrixTimes(item.localCoordenates, rotationTimes);
		Vector2[] globalCoordenates = GlobalizeCoordenatesAndFindAverage(localCoordenates, itemPositions[i], ref sum);

		//average local position and globalize to array of containers
		//then globalize to world coordenates adding holderContainers.position
		Vector3 position = sum * 0.5f + itemPositions[i];
		position.z = position.y;
		position.y = 0;
		position += gridBuilder.instances[0, 0].transform.position;

		int xIndex = (int)itemPositions[i].x;
		int yIndex = (int)itemPositions[i].y;
	}*/

	void UpdateCoordenatesInOccupancyMap(Vector2[] coordenates, bool state){
		for(int i=0; i<coordenates.Length; i++){
			occupancyMap[(int)coordenates[i].x, (int)coordenates[i].y] = state;
			Debuger("Updated " + coordenates[i]);
		}
		
		Debuger("Updated " + coordenates.Length + " coordenates in occupancy map to the state: " + state);
	}
	
    public bool showDebugs=true; void Debuger(string text) { if (showDebugs) Debug.Log(text); }

}
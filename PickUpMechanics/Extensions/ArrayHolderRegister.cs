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
	}

	public void SetupContainers(){

		occupancyMap = new bool[containers.GetLength(0), containers.GetLength(1)];

		Debuger("Containers Setted up");
	}
	
	
	public Vector2[] localCoordenates;
	void OnPickUp(){
		Pickupable item = PickUpMechanics.targetPickupable;
		Container container = PickUpMechanics.targetContainer;

		Vector2[] globalCoordenates = item.coordenates;
		Vector2 indexCoordenates = container.coordenates;
		UpdateCoordenatesInOccupancyMap(globalCoordenates, PickUpMechanics.free);

		localCoordenates = LocalizeCoordenates(globalCoordenates, indexCoordenates);
	}
	
	void OnDrop(){
		Pickupable item = PickUpMechanics.handObject;
		Container container = PickUpMechanics.targetContainer;
		
		Vector2 indexCoordenates = container.coordenates;
		Vector2[] globalCoordenates = GlobalizeCoordenates(localCoordenates, indexCoordenates);
		ArrayDebuger(localCoordenates, "Local coordenates test");
		
		item.SetCoordenates( globalCoordenates );
		UpdateCoordenatesInOccupancyMap(globalCoordenates, PickUpMechanics.occupied);
		
        item.transform.position = ContainerToWorldSpacePosition( container );
        item.transform.parent = container.transform;
		
	}
	
	public Vector2[] GlobalizeCoordenates(Vector2[] localCoordenates, Vector2 referenceCoordenate){
		Vector2[] globalizedCoordenates = new Vector2[localCoordenates.Length];
		localCoordenates.CopyTo(globalizedCoordenates, 0);
		
		globalizedCoordenates = ReferenceCoordenates(globalizedCoordenates, referenceCoordenate);
		ArrayDebuger(globalizedCoordenates, "Globalized coordenates of item in hand");
		return globalizedCoordenates;
	}

	Vector2[] LocalizeCoordenates(Vector2[] globalCoordenates, Vector2 referenceCoordenate)
    {
		ReferenceCoordenates(globalCoordenates, -referenceCoordenate);
		ArrayDebuger(globalCoordenates, "Localized coordenates of item in hand");
		return globalCoordenates;
	}
	
	Vector2[] ReferenceCoordenates(Vector2[] coordenates, Vector2 referenceCoordenate)
    {
		Vector2[] localizedCoordenates = new Vector2[coordenates.Length];
		localizedCoordenates = coordenates;

		for (int i=0; i< coordenates.Length; i++)
        {
			localizedCoordenates[i] += referenceCoordenate;
		}

		return localizedCoordenates;
	}

	Vector2 AverageOfCoordenates(Vector2[] coordenates){
		Vector2 averageOfLocalCoordenates = Vector2.zero;
		for (int i=0; i< coordenates.Length; i++)
        {
			averageOfLocalCoordenates += coordenates[i];
		}
		
		averageOfLocalCoordenates /= coordenates.Length;

		return averageOfLocalCoordenates;
	}
	
	Vector3 ContainerToWorldSpacePosition(Container container){
		Vector3 average = AverageOfCoordenates(localCoordenates);
		average.z = average.y;
		average.y = 0;
		Vector3 worldSpacePosition = average + container.transform.position;
		
		Debuger("average of placed object: " + average + "; worldSpacePosition: " + worldSpacePosition);
		return worldSpacePosition;
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
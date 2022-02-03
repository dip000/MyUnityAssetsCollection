using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ArrayHolderRegister : MonoBehaviour {

	public static Container[,] containers;
	public static bool[,] occupancyMap;
	public Vector2[] localCoordenates;
	
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
		UpdateMapFromMyHolders();

		Debuger("Containers Setted up");
	}

	void UpdateMapFromMyHolders()
	{
		List<Vector2> currentCordenates = new List<Vector2>();

		//Loops through all containers to find objects inside, if find,
		//its object inside has the coordenates for the rest of its own positions
		for (int i = 0; i < containers.GetLength(0); i++)
		{
			for (int j = 0; j < containers.GetLength(1); j++)
			{
				Pickupable itemFound = containers[i, j].objectInside;
				if (itemFound != null)
				{
					currentCordenates.InsertRange(0, itemFound.coordenates);
				}
			}
		}

		Debuger("ArrayHolderRegister found " + currentCordenates.Count + " items placed inside");

		UpdateCoordenatesInOccupancyMap(currentCordenates.ToArray(), PickUpMechanics.occupied);
	}




	

	//------------------------ LIVE REGISTER -------------------------------------
	void OnDrop(){
		Pickupable item = PickUpMechanics.handObject;
		Container container = PickUpMechanics.targetContainer;
		
		Vector2 indexCoordenates = container.coordenates;
		Vector2 volumeAverage = VolumeAverageOfCoordenates(localCoordenates);
		Vector2 volumeAverageRounded = RoundVector(volumeAverage);
		Vector2 indexCoordenatesDisplaced = indexCoordenates - volumeAverageRounded;

		Vector2[] globalCoordenates = GlobalizeCoordenates(localCoordenates, indexCoordenatesDisplaced);
		ArrayDebuger(localCoordenates, "Local coordenates test");
		
		item.SetCoordenates( globalCoordenates, indexCoordenatesDisplaced);
		UpdateCoordenatesInOccupancyMap(globalCoordenates, PickUpMechanics.occupied);
		
        item.transform.position = ContainerCoordenatesToWorldSpacePosition(indexCoordenatesDisplaced);
        item.transform.parent = null;
		
	}
	void OnPickUp()
	{
		Pickupable item = PickUpMechanics.targetPickupable;

		Vector2[] globalCoordenates = item.coordenates;
		UpdateCoordenatesInOccupancyMap(globalCoordenates, PickUpMechanics.free);

		localCoordenates = LocalizeCoordenates(globalCoordenates, item.coordenateIndex);
	}


	// ------------------------------ UTILITIES ------------------------------------
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

	Vector2 VolumeAverageOfCoordenates(Vector2[] coordenates){
		Vector2 averageOfLocalCoordenates = Vector2.zero;
		for (int i=0; i< coordenates.Length; i++)
        {
			averageOfLocalCoordenates += coordenates[i];
		}
		
		averageOfLocalCoordenates /= coordenates.Length;

		return averageOfLocalCoordenates;
	}

	Vector2 RoundVector(Vector2 vector)
    {
		vector.x = (float)System.Math.Round(vector .x);
		vector.y = (float)System.Math.Round(vector.y);
		return vector;
	}

	Vector2 BoundingBoxAverageOfCoordenates(Vector2[] coordenates){

		Vector2 averageOfLocalCoordenates = Vector2.zero;

		for (int i=0; i< coordenates.Length; i++)
        {

			if(coordenates[i].x > averageOfLocalCoordenates.x)
            {
				averageOfLocalCoordenates.x = coordenates[i].x;
			}
			if(coordenates[i].y > averageOfLocalCoordenates.y)
            {
				averageOfLocalCoordenates.y = coordenates[i].y;
			}
		}

		averageOfLocalCoordenates *= 0.5f;

		return averageOfLocalCoordenates;
	}
	
	Vector3 ContainerCoordenatesToWorldSpacePosition(Vector2 containerCoordenates){
		Vector3 containerSpacePosition = BoundingBoxAverageOfCoordenates(localCoordenates) + containerCoordenates;
		containerSpacePosition.z = containerSpacePosition.y;
		containerSpacePosition.y = 0;
		Vector3 worldSpacePosition = containerSpacePosition;
		
		Debuger("average of placed object: " + containerSpacePosition + "; worldSpacePosition: " + worldSpacePosition);
		return worldSpacePosition;
	}

	public Vector2[] GetGlobalCoordenates()
    {
		Container container = PickUpMechanics.targetContainer;

		Vector2 indexCoordenates = container.coordenates;
		Vector2 volumeAverage = VolumeAverageOfCoordenates(localCoordenates);
		Vector2 volumeAverageRounded = RoundVector(volumeAverage);
		Vector2 indexCoordenatesDisplaced = indexCoordenates - volumeAverageRounded;

		Vector2[] globalCoordenates = GlobalizeCoordenates(localCoordenates, indexCoordenatesDisplaced);
		return globalCoordenates;
	}
	public Vector2[] GetLocalCoordenates(Vector2[] localCoordenatesReference)
    {
		Container container = PickUpMechanics.targetContainer;

		Vector2 indexCoordenates = container.coordenates;
		Vector2 volumeAverage = VolumeAverageOfCoordenates(localCoordenatesReference);
		Vector2 volumeAverageRounded = RoundVector(volumeAverage);
		Vector2 indexCoordenatesDisplaced = indexCoordenates - volumeAverageRounded;

		Vector2[] localizedCoordenates = LocalizeCoordenates(localCoordenates, indexCoordenatesDisplaced);
		return localizedCoordenates;
	}

	public void RotateObjectOnHand(int times)
    {

	}

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
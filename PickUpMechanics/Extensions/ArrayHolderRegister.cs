using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ArrayHolderRegister : MonoBehaviour {

	[HideInInspector] public Container[] containers;
	public static bool[,] occupancyMap;
	
	const bool occupied = true;
	const bool free = false;
	const int coordenateDimentions = 2;
	
	int xLength;
	int yLength;
	
	void Awake(){
		PickUpMechanics.OnPickUp += OnPickUp;
		PickUpMechanics.OnDrop += OnDrop;
		
	}
	
	void Start(){
		StartCoroutine(Co_SetupContainers());
	}
	
	IEnumerator Co_SetupContainers(){
		
		//1. Get all Containers in scene
		containers = FindObjectsOfType<Container>();
		if(containers.Length <= 0){
			Debug.LogWarning("ARRAY HOLDER REGISTER. No containers in scene");
			yield break;
		}
		Debuger("Found " + containers.Length + " containers in scene");


		// 2. Pass Container's control to external components 
		for(int i=0; i<containers.Length; i++){
			containers[i].YieldControlToExternal();
		}


		// 2. Containers will take some time to initialize by themselves
		Debuger("Waiting for containers to initialize.. ");
		while( GetContainersStatus() == false){
			yield return null;
		}
		Debuger("All Containers initialized");
		
		
		// 3. Stuff happens
		FindObjecCoordenates();
		BuildOccupancyArray();
		//UpdateCoordenatesInOccupancyMap();
		
		Debuger("Containers Setted up");
	}
		
	bool GetContainersStatus(){
		bool containersFinishedInitializing = true;
		
		//TODO: Maybe use a push-pull register to keep track of all status
		for(int i=0; i<containers.Length; i++){
			containersFinishedInitializing = (containersFinishedInitializing) && (containers[i].finishedInitializing);
		}
		
		return containersFinishedInitializing;
	}
	
	void FindObjecCoordenates(){

		Dictionary< Pickupable, List<Vector3> > objects = new Dictionary< Pickupable, List<Vector3> >();
		
		
		//Saves objects like:
		//	{ object1:[(0,0)], object2:[(0,1),(1,1)], .. }
		//  Every vector is a point that defines object's shape
		for(int i=0; i<containers.Length; i++){
			
			Container currentContainer = containers[i];
			Pickupable currentItem = currentContainer.objectInside;
						
			if(currentItem != null){
				if( objects.ContainsKey(currentItem) == false ){
					objects[currentItem] = new List<Vector3>();
				}
				
				objects[currentItem].Add(currentContainer.coordenates);
				
				Debuger("Coordenate finder. Saved item " + currentItem.myName + " at Container " + currentContainer.coordenates);
			}
		}
		
		//Saves coordenates in the objects themselves
		//obj.Key is an actual reference to an object's transform
		//TODO: position the object in the average of all its containers
		foreach(var obj in objects)
		{
		  obj.Key.coordenates = obj.Value.ToArray();
		  //obj.Key.transform.position = obj.Value.Aggregate(Vector3.zero, (acc, v) => acc + v) / obj.Value.Count;
		}
		
	}
	
	void BuildOccupancyArray(){
		int highestIndex=0;
		
		for(int i=0; i<containers.Length; i++){
			int currentIndex = (int)containers[i].coordenates.x;
			if(currentIndex > highestIndex){
				highestIndex = currentIndex;
			}
		}
		
		xLength = highestIndex+1;
		yLength = containers.Length - xLength;
		occupancyMap = new bool[xLength,yLength];
		
		Debuger("Occupancy array of dimentions: " + xLength + ", " + yLength );
	}

	
	void OnPickUp(){
		Pickupable item = PickUpMechanics.handObject.GetComponent<Pickupable>();
		Vector3[] coordenatesToUpdate = item.coordenates;
		UpdateCoordenatesInOccupancyMap(coordenatesToUpdate, free);

	}
	
	void OnDrop(){
		//WARNING! handObject is alreadyNull at his point
		Pickupable item = PickUpMechanics.handObject.GetComponent<Pickupable>();
		Vector3[] coordenatesToUpdate = item.coordenates;
		UpdateCoordenatesInOccupancyMap(coordenatesToUpdate, occupied);
		
		//Update object's coordenates

	}
	
	void UpdateCoordenatesInOccupancyMap(Vector3[] coordenates, bool state){
		for(int i=0; i<coordenates.Length; i++){
			occupancyMap[(int)coordenates[i].x, (int)coordenates[i].y] = state;
		}
		
		Debuger("Updated " + coordenates.Length + " coordenates in occupancy map to the state: " + state);
	}
	
    public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }

}
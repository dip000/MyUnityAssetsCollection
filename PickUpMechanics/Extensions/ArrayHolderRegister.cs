using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ArrayHolderRegister : MonoBehaviour {

	public static Container[] containers;
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

		Dictionary< Pickupable, List<Vector2> > objects = new Dictionary< Pickupable, List<Vector2> >();
		
		
		//Saves objects like:
		//	{ object1:[(0,0)], object2:[(0,1),(1,1)], .. }
		//  Every vector is a point that defines object's shape
		for(int i=0; i<containers.Length; i++){
			
			Container currentContainer = containers[i];
			Pickupable currentItem = currentContainer.objectInside;
						
			if(currentItem != null){
				if( objects.ContainsKey(currentItem) == false ){
					objects[currentItem] = new List<Vector2>();
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
		  obj.Key.SetCoordenates(obj.Value.ToArray());
		  //obj.Key.transform.position = obj.Value.Aggregate(Vector2.zero, (acc, v) => acc + v) / obj.Value.Count;
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
	
	Vector2 average;
	bool[,] CoordenatesToObjectShape(Vector2[] coordenates){
		Vector2 refCoordenate = coordenates[0];
		Vector2 smallestPosition = new Vector2(1024,1024);
		Vector2 bigestPosition = new Vector2(-1024,-1024);
		
		for(int i=0; i<coordenates.Length; i++){
			//rebuild coordenates locally
			coordenates[i] -= refCoordenate;
			
			//Find smallest position
			if(coordenates[i].x < smallestPosition.x){
				smallestPosition.x = coordenates[i].x;
			}
			if(coordenates[i].y < smallestPosition.y){
				smallestPosition.y = coordenates[i].y;
			}
			
			//Find bigest position
			if(coordenates[i].x > bigestPosition.x){
				bigestPosition.x = coordenates[i].x;
			}
			if(coordenates[i].y > bigestPosition.y){
				bigestPosition.y = coordenates[i].y;
			}
		}
		
		int xSize = (int)bigestPosition.x - (int)smallestPosition.x +1;
		int ySize = (int)bigestPosition.y - (int)smallestPosition.y +1;
		Debuger("smallestPosition: " + smallestPosition + "; bigestPosition:" + bigestPosition);
		Debuger("xSize: " + xSize + "; ySize:" + ySize);
		
		//Move coordenates to local origin and build occupancy map
		bool[,] shape = new bool[xSize, ySize];
		average = Vector2.zero;
		for(int i=0; i<coordenates.Length; i++){
			coordenates[i] += (-smallestPosition);
			Debuger("map["+i+"]: " + coordenates[i]);
			shape[(int)coordenates[i].x, (int)coordenates[i].y] = true;
			
			average += coordenates[i];
		}
		
		//discrete center of volume
		average /= coordenates.Length;
		Debuger("average: " + average);
		average.x = (average.x-(int)average.x<0.5f)?(int)average.x:(int)average.x+1;
		average.y = (average.y-(int)average.y<0.5f)?(int)average.y:(int)average.y+1;
		Debuger("discrete average: " + average);
		return shape;
	}

	
	void OnPickUp(){
		Pickupable item = PickUpMechanics.handObject.GetComponent<Pickupable>();
		Vector2[] coordenatesToUpdate = item.coordenates;
		UpdateCoordenatesInOccupancyMap(coordenatesToUpdate, free);
		CoordenatesToObjectShape(coordenatesToUpdate);
	}
	
	void OnDrop(){
		Pickupable item = PickUpMechanics.handObject.GetComponent<Pickupable>();
		Container container = PickUpMechanics.targetTransform.GetComponent<Container>();
		
		//Test
		Vector2[] coordenatesToUpdate = new Vector2[1];
		coordenatesToUpdate[0] = container.coordenates;
		//CoordenatesToObjectShape(coordenatesToUpdate);
		
		item.SetCoordenates( coordenatesToUpdate );
		UpdateCoordenatesInOccupancyMap(coordenatesToUpdate, occupied);
		
		//Update object's coordenates

	}
	
	void UpdateCoordenatesInOccupancyMap(Vector2[] coordenates, bool state){
		for(int i=0; i<coordenates.Length; i++){
			occupancyMap[(int)coordenates[i].x, (int)coordenates[i].y] = state;
		}
		
		Debuger("Updated " + coordenates.Length + " coordenates in occupancy map to the state: " + state);
	}
	
    public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }

}
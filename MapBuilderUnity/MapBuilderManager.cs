using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;


public class MapBuilderManager : MonoBehaviour
{
	[Header("Select Graphics for each item")]
	public Items[] items = new Items[0];
	[Header( "Select Map Holder for each map" )]
	public Maps[] maps;

	[Header("Settings")]
	public TextAsset configurationsFile;
    [HideInInspector] public TextAsset previousConfigurationsFile;

	public GameObject containerGraphics;
	public float gridScale = 1;

	[Header("Check to Set Up Graphics")]
	public bool showDebugs = true;

	ArrayHolderRegister arrayHolderComponent;
	public int currentMapIndex = 0;

	/////////////////////////// MANAGING //////////////////////////////////////////
	[ContextMenu("MakeLevel")]
    public void MakeLevel()
    {
		if(configurationsFile == null){
			Debug.LogWarning("No Configurations File. Go to: https://dip000.github.io/MapBuilder/MapBuilderForWeb/MapBuilderForWeb.html To make your map");
			return;
        }
		if( maps == null ){
			Debug.LogWarning("No Map to build. Go to: https://dip000.github.io/MapBuilder/MapBuilderForWeb/MapBuilderForWeb.html To make your map");
			return;
        }

		currentMapIndex = Mathf.Clamp( currentMapIndex, 0, Mathf.Max( 0, maps.Length - 1 ) );

		//Reset and set
		//try { ResetItems();} catch{} 
        SerializeConfigurations();
		PlaceContainers();
        PlaceItems();
    }

	void SerializeConfigurations()
    {
		//Configuration file syntax is {map1Info}&{mapXInfo}${item1Info}&{itemXInfo}
		string[] parsedFiles = configurationsFile.text.Split('$');
        string[] mapInfo = parsedFiles[0].Split('&');
        string[] itemInfo = parsedFiles[1].Split('&');
		int numberOfItems = itemInfo.Length;
		int numberOfMaps = mapInfo.Length;

		//Serialize Map info
		Maps[] tempMaps = new Maps[numberOfMaps];
		for( int i = 0; i < numberOfMaps; i++ ) { 
			tempMaps[i] = Maps.CreateFromJSON( mapInfo[i] );

			if( i < maps.Length )
				tempMaps[i].mapHolder = maps[i].mapHolder;
		}
		maps = tempMaps;

		//Serialize Items info:
		// 1. Creates in a temporal instance to serialize and place user graphics
		Items[] tempItems = new Items[numberOfItems];
        for (int i = 0; i < numberOfItems; i++)
        {
            tempItems[i] = Items.CreateFromJSON( itemInfo[i] );

			// 2. Cut all graphics that desn't fit anymore
			if (i < items.Length)
				tempItems[i].graphics = items[i].graphics;
		}

		// 3. Return contents original instance
		items = tempItems;
    }

	private void OnValidate()
    {
		//If user inputed a configurations file. Serialize configurations so user can
		//place the graphics in its corresponding items
		if (configurationsFile == previousConfigurationsFile) return;
        previousConfigurationsFile = configurationsFile;

        if (configurationsFile == null) return;
        if (Application.isPlaying) return;

        SerializeConfigurations();
        Debuger("Serialized configurations");
	}
	
////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////// ITEMS PLACER ////////////////////////////
	GameObject[] itemInstances;
	GameObject[,] holderInstances;

	public void PlaceItems(){

		Maps currentMap = maps[currentMapIndex];
		Transform mapHolder = currentMap.mapHolder;

		//Use a place holder if there's no mapHolder
		List<GameObject> placeHolders = new List<GameObject>();

		itemInstances = new GameObject[ currentMap.itemTypes.Length ];

		for (int i=0; i<currentMap.itemTypes.Length; i++){
			var itemType = currentMap.itemTypes[i];
			var item = items[ itemType ];

			//Use a place holder if there are no graphics
			if (item.graphics == null) { 
				item.graphics = GameObject.CreatePrimitive(PrimitiveType.Cube);
				item.graphics.transform.localScale = Vector3.one * gridScale;
				placeHolders.Add(item.graphics);
			}

			Vector2[] itemShape = Vector2Calculations.VectorizeComponents(item.localCoordenatesX, item.localCoordenatesY);
			Vector2 itemPosition = new Vector2(currentMap.positionsX[i], currentMap.positionsY[i]);
			
			//Rotate and Globalize coordenates
			Vector2[] itemShapeRotated = Vector2Calculations.RotateMatrixAngle(itemShape, currentMap.itemRotations[i]);
			Vector2[] globalCoordenates = Vector2Calculations.Globalize(itemShapeRotated, itemPosition);

			//Find world space position to place the item
			Vector2 boxCenter = Vector2Calculations.BoundingBoxCenterOfCoordenates( itemShapeRotated);
			Vector2 gridSpacePosition = (boxCenter + itemPosition) * gridScale;
			Vector3 worldSpacePosition = new Vector3(gridSpacePosition.x, 0, gridSpacePosition.y) + mapHolder.position;

			//Apply transforms and properties
			//NOTE: Damn rotation is inverted natively, it rotates clockwise on positive angles. So 360-angle rotates correctly
			GameObject itemInstance = Instantiate(item.graphics, worldSpacePosition, Quaternion.Euler(0, 360 - currentMap.itemRotations[i], 0) );
			itemInstance.name = item.itemName;
			itemInstance.transform.parent = mapHolder;

			//Add a Pickupable component if it didn't have one
			if( itemInstance.TryGetComponent( out Pickupable itemComponent ) == false )
				itemComponent = itemInstance.AddComponent<Pickupable>();

			arrayHolderComponent.UpdateCoordenatesInOccupancyMap( globalCoordenates, PickUpMechanics.occupied );

			//itemComponent.SetOccupancy( holderInstances[] );
			itemComponent.myName = item.itemName;
			itemComponent.SetShape( itemShape );

			//Save as a reference
			itemInstances[i] = itemInstance;
		}

		//Delete used PlaceContainers
		foreach(var placeHolder in placeHolders)
			DestroyImmediate(placeHolder);

	}
	
	[ContextMenu("ResetItems")]
	public void ResetItems(){
		if(itemInstances== null) return;
		if(itemInstances.Length <= 0) return;
		
		foreach(var instance in itemInstances){
			DestroyImmediate(instance);
		}

		itemInstances = null;
		ResetHolders();
		Debuger("Deleted items in scene");
	}


	
	void PlaceContainers(){
		var currentMap = maps[currentMapIndex];
		var mapHolder = currentMap.mapHolder;

		if( mapHolder == null ){
			mapHolder = (new GameObject( "mapHolder"+currentMapIndex )).transform;
			mapHolder.parent = transform;
			mapHolder.position = transform.position;
			currentMap.mapHolder = mapHolder;
		}

		holderInstances = new GameObject[(int)currentMap.mapSizeX, (int)currentMap.mapSizeY];
		var holder = containerGraphics;
		var usedPlaceHolders = false;

		if( holder == null ){
			holder = GameObject.CreatePrimitive( PrimitiveType.Cube );
			usedPlaceHolders = true;
		}

		//Add a ArrayHolderRegister component if it didn't have one
		if( mapHolder.TryGetComponent( out arrayHolderComponent ) == false )
			arrayHolderComponent = mapHolder.gameObject.AddComponent<ArrayHolderRegister>();

		arrayHolderComponent.Setup( new Vector2( currentMap.mapSizeX, currentMap.mapSizeY ) );

		for( int i=0; i< currentMap.mapSizeX; i++){
			for (int j=0; j< currentMap.mapSizeY; j++){

				var instance = Instantiate( holder );
				instance.name = "Holder "+i+", "+j;

				instance.transform.position = (new Vector3(i, 0, j)) * gridScale + mapHolder.position;
				instance.transform.parent = mapHolder;

				//Add a Container component if it didn't have one
				if( instance.TryGetComponent( out Container containerComponent ) == false)
					containerComponent = instance.AddComponent<Container>();

				containerComponent.coordenates = new Vector2( i, j );
				holderInstances[i, j] = instance;
			}
		}

		if( usedPlaceHolders )
			DestroyImmediate( holder );
	}
	
	void ResetHolders(){
		if(holderInstances== null) return;
		if(holderInstances.GetLength(0) <= 0) return;
		
		for (int i=0; i<holderInstances.GetLength(0); i++){
			for (int j=0; j<holderInstances.GetLength(1); j++){
				DestroyImmediate( holderInstances[i, j] );
			}
		}

		itemInstances = null;
		Debuger("Deleted Containers in scene");
	}

////////////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////// CLASSES /////////////////////////////////

	[System.Serializable]
	public class Items {
		[HideInInspector] public string itemName;
		public GameObject graphics;

		[HideInInspector] public float[] localCoordenatesX;
		[HideInInspector] public float[] localCoordenatesY;

		public static Items CreateFromJSON(string jsonString){
			return JsonUtility.FromJson<Items>(jsonString);
		}
	}

	[System.Serializable]
	public class Maps
	{
		[HideInInspector] public string mapName;
		public Transform mapHolder;
		[HideInInspector] public float mapSizeX, mapSizeY;
		[HideInInspector] public int[] itemTypes;
		[HideInInspector] public int[] itemRotations;
		[HideInInspector] public float[] positionsX, positionsY;

		public static Maps CreateFromJSON(string jsonString){
			return JsonUtility.FromJson<Maps>(jsonString);
		}
	}

////////////////////////////////////////////////////////////////////////////////////

/////////////////////// DEBUGS /////////////////////////////////////////////////////
    void Debuger(string text) { if (showDebugs) Debug.Log(text); }
	void OnDrawGizmos()
	{
		if( showDebugs == false ) return;

		//Grid origin at current map index. If there's no holder, use this builder's position
		Transform mapHolder;
		Vector3 gridOrigin;
		int itemsX;
		int itemsY;

		if( maps == null ){
			mapHolder = transform;
			itemsX = itemsY = 10;
		}
		else if( maps.Length <= 0){
			mapHolder = transform;
			itemsX = itemsY = 10;
		}
		else{
			currentMapIndex = Mathf.Clamp(currentMapIndex, 0, maps.Length-1 );
			var currentMap = maps[currentMapIndex];
			if( currentMap.mapHolder == null )
				mapHolder = transform;
			else
				mapHolder = currentMap.mapHolder;
			
			itemsX = (int)maps[currentMapIndex].mapSizeX;
			itemsY = (int)maps[currentMapIndex].mapSizeY;
		}

		gridOrigin = mapHolder.position;


		//Constants
		Gizmos.color = Color.green;
		Vector3 offset = new Vector3(0.5f, 0, 0.5f) * gridScale;

		//DRAW COLUMNS
		Vector3 cellIncrement = gridOrigin + Vector3.zero;
		Vector3 colIncrement = Vector3.forward * gridScale * itemsY;
		Vector3 rowIncrement = Vector3.right * gridScale;
		for (int i = 0; i < itemsX+1; i++)
		{
			Gizmos.DrawLine(cellIncrement - offset, colIncrement + cellIncrement - offset);
			cellIncrement += rowIncrement;
		}

		//DRAW ROWS 
		cellIncrement = gridOrigin + Vector3.zero;
		colIncrement = Vector3.right * gridScale * itemsX;
		rowIncrement = Vector3.forward * gridScale;
		for (int i = 0; i < itemsY+1; i++)
		{
			Gizmos.DrawLine(cellIncrement - offset, colIncrement + cellIncrement - offset);
			cellIncrement += rowIncrement;
		}



	}

	void ArrayDebuger( Vector2[] vectorArray, string text = "ArrayDebuger" ) { if( showDebugs ) for( int i = 0; i < vectorArray.Length; i++ ) Debug.Log( text + " [" + i + "] " + vectorArray[i] ); }

	////////////////////////////////////////////////////////////////////////////////////

	/////////////////////// TESTS /////////////////////////////////////////////////////



}



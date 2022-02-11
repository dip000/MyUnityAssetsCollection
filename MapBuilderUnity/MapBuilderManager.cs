using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;


public class MapBuilderManager : MonoBehaviour
{
	[Header("Select Graphics for each item")]
	public Items[] items = new Items[0];
	public Maps maps;

	[Header("Settings")]
	public TextAsset configurationsFile;
    [HideInInspector] public TextAsset previousConfigurationsFile;

	public Transform levelHolder;
	public float gridScale = 1;

	[Header("Check to Set Up Graphics")]
	public bool showDebugs = true;

/////////////////////////// MANAGING //////////////////////////////////////////
	[ContextMenu("MakeLevel")]
    public void MakeLevel()
    {
		if(configurationsFile == null){
			Debug.LogWarning("No Configurations File. Go to: https://dip000.github.io/MapBuilder/MapBuilderForWeb/MapBuilderForWeb.html To make your map");
			return;
        }
		
		//Reset and set
		try {ResetItems();} catch{} 
        SerializeConfigurations();
		PlaceHolders();
        PlaceItems();
    }

	void SerializeConfigurations()
    {
        //Configuration file syntax is {mapInfo}${item1Info}&{item2Info}&{...}
        string[] parsedFiles = configurationsFile.text.Split('$');
        string mapInfo = parsedFiles[0];
        string[] itemInfo = parsedFiles[1].Split('&');
		int numberOfItems = itemInfo.Length;

        //Serialize Map info
        maps = new Maps();
        maps = Maps.CreateFromJSON(mapInfo);

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

	public void PlaceItems(){

		//Use a place holder if there's no levelHolder
		List<GameObject> placeHolders = new List<GameObject>();
		if (levelHolder == null){
			levelHolder = (new GameObject("levelHolderPlaceHolder")).transform;
			levelHolder.parent = transform;
			levelHolder.position = transform.position;
		}

		itemInstances = new GameObject[ maps.itemTypes.Length ];

		for (int i=0; i<maps.itemTypes.Length; i++){
			var itemType = maps.itemTypes[i];
			var item = items[ itemType ];

			//Use a place holder if there are no graphics
			if (item.graphics == null) { 
				item.graphics = GameObject.CreatePrimitive(PrimitiveType.Cube);
				item.graphics.transform.localScale = Vector3.one * gridScale;
				placeHolders.Add(item.graphics);
			}

			Vector2[] itemShape = Vector2Calculations.VectorizeComponents(item.localCoordenatesX, item.localCoordenatesY);
			Vector2 itemPosition = new Vector2(maps.positionsX[i], maps.positionsY[i]);
			
			//Rotate and Globalize coordenates
			Vector2[] itemShapeRotated = Vector2Calculations.RotateMatrixAngle(itemShape, maps.itemRotations[i]);
			Vector2[] globalCoordenates = Vector2Calculations.Globalize(itemShapeRotated, itemPosition);

			//Find world space position to place the item
			Vector2 boxCenter = Vector2Calculations.BoundingBoxCenterOfCoordenates( itemShapeRotated);
			Vector2 gridSpacePosition = (boxCenter + itemPosition) * gridScale;
			Vector3 worldSpacePosition = new Vector3(gridSpacePosition.x, 0, gridSpacePosition.y) + levelHolder.position;

			//Apply transforms and properties
			//NOTE: Damn rotation is inverted natively, it rotates clockwise on positive angles. So 360-angle rotates correctly
			GameObject itemInstance = Instantiate(item.graphics, worldSpacePosition, Quaternion.Euler(0, 360 - maps.itemRotations[i], 0) );
			itemInstance.name = item.itemName;
			itemInstance.transform.parent = levelHolder;

			//Save as a reference
			itemInstances[i] = itemInstance;
			//Debuger("Placed " + item.itemName + " in position " + itemPosition + " and rotation " + maps.itemRotations[i]);
		}

		//Delete used PlaceHolders
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


	
	GameObject[,] holderInstances;
	void PlaceHolders(){
		holderInstances = new GameObject[(int)maps.mapSizeX, (int)maps.mapSizeY];
		for (int i=0; i<maps.mapSizeX; i++){
			for (int j=0; j<maps.mapSizeY; j++){
				var holder = GameObject.CreatePrimitive(PrimitiveType.Cube);
				holder.name = "Holder "+i+", "+j;
				holder.AddComponent<Container>();
				holder.transform.position = (new Vector3(i, 0, j)) * gridScale + levelHolder.position;
				holder.transform.parent = levelHolder;
				
				var containerComponent = holder.GetComponent<Container>();
				containerComponent.coordenates = new Vector2(i, j);
				
				holderInstances[i, j] = holder;
			}
		}
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
		Debuger("Deleted items in scene");
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

	public class Maps
	{
		public string mapName;
		public float mapSizeX, mapSizeY;
		public int[] itemTypes;
		public int[] itemRotations;
		public float[] positionsX, positionsY;

		public static Maps CreateFromJSON(string jsonString){
			return JsonUtility.FromJson<Maps>(jsonString);
		}
	}

////////////////////////////////////////////////////////////////////////////////////

/////////////////////// DEBUGS /////////////////////////////////////////////////////
    void Debuger(string text) { if (showDebugs) Debug.Log(text); }
	void OnDrawGizmos()
	{
		if (showDebugs == false) return;
		
		//Grid origin
		Vector3 gridOrigin;
		if (levelHolder == null)
			gridOrigin = transform.position;
		else
			gridOrigin = levelHolder.position;

		//Grid Size
		int itemsX;
		int itemsY;
		if (maps == null)
			itemsX = itemsY = 10;
		else{
			itemsX = (int)maps.mapSizeX;
			itemsY = (int)maps.mapSizeY;
		}

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


////////////////////////////////////////////////////////////////////////////////////

/////////////////////// TESTS /////////////////////////////////////////////////////



}



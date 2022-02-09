using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapBuilderManager : MonoBehaviour
{
    public TextAsset configurationsFile;
    [HideInInspector] public TextAsset previousConfigurationsFile;

    public Items[] items;
    public Maps maps;
	
	public Transform levelHolder;

/////////////////////////// MANAGING //////////////////////////////////////////
    [ContextMenu("MakeLevel")]
    public void MakeLevel()
    {
		try {ResetItems();} catch{} 
		
        //Inspector doesnt remember configurations, so update again
        SerializeConfigurations();

        //Give build instruction
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


        //Serialize Items info
        //Creates from JSON then override graphics. Clamp all graphics that desn't fit anymore
        Items[] tempItems = new Items[numberOfItems];
        for (int i = 0; i < numberOfItems; i++)
        {
            tempItems[i] = Items.CreateFromJSON( itemInfo[i] );
			if(i < items.Length)
				tempItems[i].graphics = items[i].graphics;
		}
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

		//ResetItems();
		itemInstances = new GameObject[ maps.itemTypes.Length ];
		
		for(int i=0; i<maps.itemTypes.Length; i++){
			var itemType = maps.itemTypes[i];
			var item = items[ itemType ];
			var graphics = item.graphics;

			Vector2[] itemShape = MapBuilderCalculations.VectorizeComponents(item.localCoordenatesX, item.localCoordenatesY);
			Vector2 itemPosition = new Vector2(maps.positionsX[i], maps.positionsY[i]);
			
			//Rotate and Globalize coordenates
			Vector2[] itemShapeRotated = MapBuilderCalculations.RotateMatrixAngle(itemShape, maps.itemRotations[i]);
			Vector2[] globalCoordenates = MapBuilderCalculations.GlobalizeCoordenates(itemShapeRotated, itemPosition);

			//Find world space position to place the item
			Vector2 boxCenter = MapBuilderCalculations.BoundingBoxAverageOfCoordenates(itemShapeRotated);
			Vector2 gridSpacePosition = boxCenter + itemPosition;
			Vector3 worldSpacePosition = new Vector3(gridSpacePosition.x, 0, gridSpacePosition.y) + levelHolder.position;

			//Apply transforms and properties
			//Goddamn rotation is inverted natively, it rotates clockwise on positive angles. So 360-angle rotates correctly
			GameObject itemInstance = Instantiate(graphics, worldSpacePosition, Quaternion.Euler(0, 360 - maps.itemRotations[i], 0) );
			itemInstance.name = item.itemName;

			//Save as a reference
			itemInstances[i] = itemInstance;
			Debuger("Placed " + item.itemName + " in position " + itemPosition + " and rotation " + maps.itemRotations[i]);
		}

	}
	
	[ContextMenu("Reset Items")]
	void ResetItems(){
		if(itemInstances== null)
			return;
		
		if(itemInstances.Length <= 0)
			return;
		
		foreach(var instance in itemInstances){
			DestroyImmediate(instance);
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

    public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }
}

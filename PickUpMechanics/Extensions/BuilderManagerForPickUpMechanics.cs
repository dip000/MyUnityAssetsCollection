using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*  STATUS: Basic functioning for only 1 map at a time. Under design tests
 * 
 *  TODO:
 *      1. Figure a way to build several maps (maybe changing holderOfContainers and resetting up)
 *      2. Figure a way to read several maps from JSON file (maybe using string[] maps = mapsFile.text.Split(',');)
 *      3. Implement an item shaper externally to serialize with 'itemsFile'
*/

[RequireComponent(typeof(GridBuilder2D))]
[RequireComponent(typeof(ItemPlacerGrid2D))]
[RequireComponent(typeof(RotateObjectOnHand))]
[RequireComponent(typeof(PickUpMechanics))]
public class BuilderManagerForPickUpMechanics : MonoBehaviour
{
    public TextAsset mapsFile;
    public TextAsset itemsFile;
    public TextAsset configurationsFile;

    public string projectName;
    public int numberOfMaps;
    public int numberOfItems;

    public ItemPlacerGrid2D.Items[] items;
    public Maps[] maps;

    GridBuilder2D gridBuilder2D;
    ItemPlacerGrid2D itemPlacerGrid2D;
	ArrayHolderRegister arrayHolderRegister;
	RotateObjectOnHand rotateObjectOnHand;
	
	void Awake()
    {
        FindComponents();
    }
    private void FindComponents()
    {
        gridBuilder2D = GetComponent<GridBuilder2D>();
        itemPlacerGrid2D = GetComponent<ItemPlacerGrid2D>();
        rotateObjectOnHand = GetComponent<RotateObjectOnHand>();
    }

    [ContextMenu("Build Test")]
    void BuildTest()
    {
        FindComponents();
        //UNDER TESTING////
        if (configurationsFile)
        {
            UseExternalConfigurations();
        }
        ///////////////////

        MakeLevel(0);
    }


    void UseExternalConfigurations()
    {
        //Configuration file syntax is {mapInfo}${item1Info}&{item2Info}&{...}
        string[] parsedFiles = configurationsFile.text.Split('$');
        string mapInfo = parsedFiles[0];
        string[] itemInfo = parsedFiles[1].Split('&');
        int numberOfItems = itemInfo.Length;

        //Accepts only one map right now
        if (maps.Length != 0)
            maps = new Maps[1];

        maps[0] = Maps.CreateFromJSON(mapInfo);
        maps[0].ReplaceInspectorData();

        //Items declared, will use its graphics. Items not declared will create them from scratch
        for (int i=0; i< numberOfItems; i++)
        {
            
        }

        if (items.Length == 0)
            items = new ItemPlacerGrid2D.Items[numberOfItems-1]; //NOTE: Minus 1 because Output JSON creates an "&" at the end that interprets it as another item but that is a bug from WebBuilder

        items[0] = ItemPlacerGrid2D.Items.CreateFromJSON(itemInfo[0]);
        items[0].ReplaceInspectorData();
    }

    public void MakeLevel(int level)
    {
		if( SeccurityChecks(level) == false )
        {
            Debuger("Did not make a level because of previous error");
            return;
        }

		//Setup builders
		float margin = 0.1f;
		Vector3 containersSize = new Vector3(0.9f, 0.9f, 0.9f);
		
        gridBuilder2D.Setup(maps[level].mapSize, containersSize, margin);
        //itemPlacerGrid2D.Setup(items, maps[level].itemTypes, maps[level].itemPositions, maps[level].itemRotations);

		//Give build instruction
        gridBuilder2D.BuildGrid();
        itemPlacerGrid2D.PlaceItems();

		//Start registering item pickups and drops
        arrayHolderRegister = gridBuilder2D.graphicsParent.GetComponent<ArrayHolderRegister>();
		arrayHolderRegister.RegisterContainers(gridBuilder2D.instances);
        arrayHolderRegister.SetupContainers();
		
		//Enable object rotations using selected register
		rotateObjectOnHand.StartObjectRotations(arrayHolderRegister);
    }


	bool SeccurityChecks(int level){

        //Hard Checks
        if(level > maps.Length)
        {
            Debug.LogError("BUILDER MANAGER. There's no level " + level + " registered in neither inspector or file. Forgot to place a 'Maps File' in inspector?");
            return false;
        }

        //Look ups
        Vector2[] itemPositions = maps[level].itemPositions;
        Vector2 mapSize = maps[level].mapSize;
        int numberOfinstructions = maps[level].itemTypes.Length;

        //Soft Checks
        if (itemPositions == null)
        {
            Debug.LogWarning("BUILDER MANAGER. itemPositions has no values. Placing one at origin");
            maps[level].itemPositions[0] = Vector2.zero;
        }

        if (mapSize == Vector2.zero)
        {
            Debug.LogWarning("BUILDER MANAGER. mapSize is zero. Will build a 10x10 map");
            maps[level].mapSize = new Vector2(10,10);
        }

        if (itemPositions.Length != numberOfinstructions)
        {
            Debug.LogWarning("BUILDER MANAGER. itemPositions has " + itemPositions.Length + " out of " + numberOfinstructions + " positions");
        }
        if (maps[level].itemRotations.Length != numberOfinstructions)
        {
            Debug.LogWarning("BUILDER MANAGER. itemRotations has " + maps[level].itemRotations.Length + " out of " + numberOfinstructions + " rotations");
        }

        return true;
	}


    [System.Serializable]
    public class Maps
    {
        public string mapName;
        public int[] itemTypes;
        public int[] itemRotations;
        public Vector2[] itemPositions;
        public Vector2 mapSize;

        [HideInInspector] public float mapSizeX;
        [HideInInspector] public float mapSizeY;

        [HideInInspector] public float[] positionsX;
        [HideInInspector] public float[] positionsY;

        public void ReplaceInspectorData()
        {
            ReplaceItemPositions();
            ReplaceMapSize();
        }

        public static Maps CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<Maps>(jsonString);
        }

        void ReplaceMapSize()
        {
            mapSize = new Vector2(mapSizeX, mapSizeY);
        }

        void ReplaceItemPositions()
        {
            itemPositions = new Vector2[positionsX.Length];
            for (int i=0; i < positionsX.Length; i++)
            {
                itemPositions[i].x = positionsX[i];
                itemPositions[i].y = positionsY[i];
            }
        }
    }
    public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }
}

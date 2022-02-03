using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(GridBuilder2D))]
[RequireComponent(typeof(ItemPlacerGrid2D))]
[RequireComponent(typeof(RotateObjectOnHand))]
public class BuilderManagerForPickUpMechanics : MonoBehaviour
{
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
	
	
    private void Awake()
    {
        gridBuilder2D = GetComponent<GridBuilder2D>();
        itemPlacerGrid2D = GetComponent<ItemPlacerGrid2D>();
        rotateObjectOnHand = GetComponent<RotateObjectOnHand>();
    }

    void Start()
    {
        MakeLevel(0);
    }


    public void MakeLevel(int level)
    {
		SeccurityChecks(level);

		//Setup builders
		float margin = 0.1f;
		Vector3 containersSize = new Vector3(0.9f, 0.9f, 0.9f);
		
        gridBuilder2D.Setup(maps[level].mapSize, containersSize, margin);
        itemPlacerGrid2D.Setup(items, maps[level].itemInstructionIndexes, maps[level].itemPositions, maps[level].itemRotations);

		//Give build instruction
        gridBuilder2D.BuildGrid();
        itemPlacerGrid2D.PlaceItems();

		//Start registering item pickups and drops
        arrayHolderRegister = gridBuilder2D.graphicsParent.GetComponent<ArrayHolderRegister>();
		arrayHolderRegister.RegisterContainers(gridBuilder2D.instances);
        arrayHolderRegister.SetupContainers();
		
		//Enable object rotations
		rotateObjectOnHand.StartObjectRotations(arrayHolderRegister);
    }


	void SeccurityChecks(int level){
        int numberOfinstructions = maps[level].itemInstructionIndexes.Length;
		
        if (maps[level].itemPositions.Length != numberOfinstructions)
        {
            Debug.LogWarning("INSTRUCTION INTERPRETER. itemPositions has " + maps[level].itemPositions.Length + " out of " + numberOfinstructions + " positions. Must be same");
        }
        if (maps[level].itemRotations.Length != numberOfinstructions)
        {
            Debug.LogWarning("INSTRUCTION INTERPRETER. itemRotations has " + maps[level].itemRotations.Length + " out of " + numberOfinstructions + " rotations. Must be same");
        }	
	}
	

    [System.Serializable]
    public class Maps
    {
        public string mapName;
        public int[] itemInstructionIndexes;
        public Vector2[] itemPositions;
        public int[] itemRotations;
        public Vector2 mapSize;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionInterpreter : MonoBehaviour
{
    public TextAsset configurationsFile;

    public string projectName;
    public int numberOfMaps;
    public int numberOfItems;

    public ItemPlacerGrid2D.Items[] items;
    public Maps[] maps;

    GridBuilder2D gridBuilder2D;
    ItemPlacerGrid2D itemPlacerGrid2D;

    // Start is called before the first frame update
    private void Awake()
    {
        gridBuilder2D = GetComponent<GridBuilder2D>();
        itemPlacerGrid2D = GetComponent<ItemPlacerGrid2D>();
    }

    void Start()
    {
        MakeLevel(0);
    }

    public void MakeLevel(int level)
    {
        int instructions = maps[level].itemInstructionIndexes.Length;

        if (maps[level].itemPositions.Length != instructions)
        {
            Debug.LogWarning("INSTRUCTION INTERPRETER. itemPositions has " + maps[level].itemPositions.Length + " out of " + instructions + " positions. Must be same");
        }
        if (maps[level].itemRotations.Length != instructions)
        {
            Debug.LogWarning("INSTRUCTION INTERPRETER. itemRotations has " + maps[level].itemRotations.Length + " out of " + instructions + " rotations. Must be same");
        }

        gridBuilder2D.Setup(maps[level].mapSize, new Vector2(0.9f, 0.9f), 0.1f);
        itemPlacerGrid2D.Setup(items, maps[level].itemInstructionIndexes, maps[level].itemPositions, maps[level].itemRotations);

        gridBuilder2D.BuildGrid();
        itemPlacerGrid2D.PlaceItems();

        ArrayHolderRegister arrayHolderRegister = gridBuilder2D.graphicsParent.GetComponent<ArrayHolderRegister>();
        arrayHolderRegister.SetupContainers();
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

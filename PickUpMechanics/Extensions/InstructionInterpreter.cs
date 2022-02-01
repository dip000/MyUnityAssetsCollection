using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionInterpreter : MonoBehaviour
{
    public TextAsset project;

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
        gridBuilder2D.Setup(maps[0].size, new Vector2(0.5f, 0.5f), 0.5f);

        itemPlacerGrid2D.Setup(items);

        gridBuilder2D.BuildGrid();
        itemPlacerGrid2D.PlaceItems();
    }


    [System.Serializable]
    public class Maps
    {
        public string mapName;
        public int[] itemInstructionIndex;
        public Vector2[] itemPositionIndex;
        public Vector2 size;
    }
}

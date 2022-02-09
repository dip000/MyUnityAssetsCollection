using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridBuilder2D))]
[RequireComponent(typeof(ItemPlacerGrid2D))]
public class BuilderManagerWithConfigurationsFile : MonoBehaviour
{
    public TextAsset configurationsFile;
    [HideInInspector] public TextAsset previousConfigurationsFile;

    public ItemPlacerGrid2D.Items[] items;
    public GridBuilder2D.Maps maps;

    GridBuilder2D gridBuilder2D;
    ItemPlacerGrid2D itemPlacerGrid2D;



    private void FindComponents()
    {
        gridBuilder2D = GetComponent<GridBuilder2D>();
        itemPlacerGrid2D = GetComponent<ItemPlacerGrid2D>();
    }

    // Update is called once per frame
    void SerializeConfigurations()
    {
        //Configuration file syntax is {mapInfo}${item1Info}&{item2Info}&{...}
        string[] parsedFiles = configurationsFile.text.Split('$');
        string mapInfo = parsedFiles[0];
        string[] itemInfo = parsedFiles[1].Split('&');
        int numberOfItems = itemInfo.Length;

        //Serialize Map info
        maps = new GridBuilder2D.Maps();
        maps = GridBuilder2D.Maps.CreateFromJSON(mapInfo);
        maps.ReplaceInspectorData();

        //Save user graphics if there's any
        GameObject[] tempItemGraphics = new GameObject[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            tempItemGraphics[i] = items[i].graphics;
        }

        //Serialize Items info
        items = new ItemPlacerGrid2D.Items[numberOfItems];
        for (int i = 0; i < numberOfItems; i++)
        {
            items[i] = ItemPlacerGrid2D.Items.CreateFromJSON(itemInfo[i]);
            items[i].ReplaceInspectorData();
        }

        //Return user graphics, clamp all graphics that desn't fit anymore
        for (int i = 0; i < tempItemGraphics.Length; i++)
        {
            if (i >= items.Length)
                break;

            items[i].graphics = tempItemGraphics[i];
        }
    }

    [ContextMenu("MakeLevel")]
    public void MakeLevel()
    {
        //Inspector doesnt remember configurations, so update again
        SerializeConfigurations();
        FindComponents();

        if (SeccurityChecks() == false)
        {
            Debuger("Did not make a level because of previous error");
            return;
        }

        //Setup builders
        float margin = 0.1f;
        Vector3 containersSize = new Vector3(0.9f, 0.9f, 0.9f);

        gridBuilder2D.Setup(maps.mapSize, containersSize, margin);
        itemPlacerGrid2D.SetupItems(items);
        itemPlacerGrid2D.ReadFromMap(maps);

        //Give build instruction
        gridBuilder2D.BuildGrid();
        itemPlacerGrid2D.PlaceItems();
    }


    bool SeccurityChecks()
    {
        //Look ups
        Vector2[] itemPositions = maps.itemPositions;
        Vector2 mapSize = maps.mapSize;
        int numberOfinstructions = maps.itemTypes.Length;

        //Soft Checks
        if (itemPositions == null)
        {
            Debug.LogWarning("BUILDER MANAGER. itemPositions has no values. Placing one at origin");
            maps.itemPositions[0] = Vector2.zero;
        }

        if (mapSize == Vector2.zero)
        {
            Debug.LogWarning("BUILDER MANAGER. mapSize is zero. Will build a 10x10 map");
            maps.mapSize = new Vector2(10, 10);
        }

        if (itemPositions.Length != numberOfinstructions)
        {
            Debug.LogWarning("BUILDER MANAGER. itemPositions has " + itemPositions.Length + " out of " + numberOfinstructions + " positions");
        }
        if (maps.itemRotations.Length != numberOfinstructions)
        {
            Debug.LogWarning("BUILDER MANAGER. itemRotations has " + maps.itemRotations.Length + " out of " + numberOfinstructions + " rotations");
        }

        return true;
    }

    private void OnValidate()
    {
        //If user inputed a configurations file. Serialize configurations so user can
        //place the graphics in its corresponding items

        if (configurationsFile == previousConfigurationsFile)
        {
            return;
        }
        previousConfigurationsFile = configurationsFile;

        if (configurationsFile == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            return;
        }

        SerializeConfigurations();
        Debuger("Serialized configurations");
    }

    public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * TODO:
 *		1. 
*/

public class GridBuilder2D : MonoBehaviour
{
	public Maps[] maps;

	public Vector2 containers = new Vector2(3,2);
	public Vector3 containersSize = new Vector3(0.8f,0.8f, 0.8f);
	public float margin = 0.2f;

	public GameObject containerGraphics;
	public GameObject holderOfContainers;

	public Container[,] instances {get; private set;}

	public GameObject graphicsParent;
	
    private void Awake()
    {
	}

	public void Setup(Vector2 _containers, Vector3 _containersSize, float _margin)
    {
		containers = _containers;
		containersSize = _containersSize;
		margin = _margin;
	}

    [ContextMenu("Build Grid")]
	public void BuildGrid(){
		Debuger("Building Grid..");		
		
		var placeHolder = GameObject.CreatePrimitive(PrimitiveType.Cube);
		var graphics = containerGraphics;
		graphicsParent = holderOfContainers;
		instances = new Container[ (int)containers.x,(int)containers.y ];
		
		
		if(containerGraphics == null){
			Debuger("No graphics to use as containers. Will use placeholders");
			graphics = placeHolder;
			graphics.AddComponent<Container>();
			graphics.name = "Original Container";
		}
		
		if(holderOfContainers == null){
			Debuger("No holderContainer. Will make one at World Origin");
			graphicsParent = new GameObject("Holder Of Containers");
		}

		for (int i=0; i<containers.x; i++){			
			for(int j=0; j<containers.y; j++){
				GameObject instance = Instantiate(graphics, graphicsParent.transform);
				instance.transform.localScale = containersSize;
				instance.name = "Container " + i + "-" + j;
				
				Vector3 objectPosition = Vector3.zero;
				objectPosition.x = i * (containersSize.x + margin);
				objectPosition.z = j * (containersSize.z + margin);
				
				instance.transform.position = objectPosition;
				
				Container instanceComponent = instance.GetComponent<Container>();
				instanceComponent.coordenates = new Vector2(i, j);
				
				instances[i, j] = instanceComponent;
			}
		}

		DestroyImmediate(placeHolder);
		Debuger("Grid Built");		
	}
	
	[ContextMenu("Reset Grid")]
	void ResetGrid(){
		if(instances == null)
			return;
		
		Debuger("Grid Reseted");
		for(int i=0; i<instances.GetLength(0); i++){			
			for(int j=0; j<instances.GetLength(1); j++){
				DestroyImmediate(instances[i,j].gameObject);
			}
		}

		DestroyImmediate(graphicsParent);
		instances = new Container[0,0];
	}

	//[System.Serializable]
	public class Maps
	{
		public string mapName;
		public int[] itemTypes;
		public int[] itemRotations;
		public Vector2[] itemPositions;
		public Vector2 mapSize;

		public float mapSizeX;
		public float mapSizeY;

		public float[] positionsX;
		public float[] positionsY;

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
			for (int i = 0; i < positionsX.Length; i++)
			{
				itemPositions[i].x = positionsX[i];
				itemPositions[i].y = positionsY[i];
			}
		}
	}
	public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }

}

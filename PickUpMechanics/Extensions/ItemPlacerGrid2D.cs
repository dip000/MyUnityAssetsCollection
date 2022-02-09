using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(GridBuilder2D))]
public class ItemPlacerGrid2D : MonoBehaviour
{
	
	public Items[] items;
	GridBuilder2D.Maps map;
	GridBuilder2D gridBuilder;
	Transform[] instances;
	
	//TEST
	public int[] itemTypes = {0};
	public Vector2[] itemPositions = {Vector2.zero};
	public int[] itemRotations = {0};

    public void SetupItems(Items[] _items)
    {
		items = _items;
	}
    public void ReadFromMap(GridBuilder2D.Maps _map)
    {
		map = _map;
		itemTypes = _map.itemTypes;
		itemPositions = _map.itemPositions;
		itemRotations = _map.itemRotations;
	}

	[ContextMenu("Place Items")]
	public void PlaceItems(){

		//ResetItems();
		instances = new Transform[ itemTypes.Length ];
		
		var placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
		placeholder.name = "Original Item";
		
		gridBuilder = GetComponent<GridBuilder2D>();
		
		for(int i=0; i<itemTypes.Length; i++){
			var itemIndex = itemTypes[i];
			var item = items[ itemIndex ];
			var graphics = item.graphics;


			if(graphics == null){
				Debuger("No graphics to use as item. Will use a placeholder");
				graphics = placeholder;
			}

			//Rotate and Globalize coordenates
			int rotationAngleClamp = itemRotations[i];
			Vector2[] localCoordenates = RotateMatrixAngle(item.localCoordenates, itemRotations[i]);
			Vector2[] globalCoordenates = GlobalizeCoordenates(localCoordenates, itemPositions[i]);
			Vector2 average = BoundingBoxAverageOfCoordenates(localCoordenates);

			//average local position and globalize to array of containers
			//then globalize to world coordenates adding holderContainers.position
			Vector3 position = average + itemPositions[i];
			position.z = position.y;
			position.y = 0;
			position += gridBuilder.instances[0, 0].transform.position;

			int xIndex = (int)itemPositions[i].x;
			int yIndex = (int)itemPositions[i].y;

			//Apply transforms and properties
			//Goddamn rotation is inverted natively, it rotates clockwise on positive angles. So 360-angle rotates correctly
			Container container = gridBuilder.instances[xIndex, yIndex];
			GameObject itemInstance = Instantiate(graphics, position, Quaternion.Euler(0, 360 - rotationAngleClamp, 0) );
			itemInstance.name = item.itemName;
			itemInstance.AddComponent<Pickupable>();

			//Setup components and internal logic
			Pickupable itemInstanceComponent = itemInstance.GetComponent<Pickupable>();

			itemInstanceComponent.myName = item.itemName;
			itemInstanceComponent.SetCoordenates( globalCoordenates, itemPositions[i]);
			itemInstanceComponent.SetOccupancy( container );
			container.SetOccupancy(itemInstanceComponent);

			//Save as a reference
			instances[i] = itemInstance.transform;
			Debuger("Placed " + item.itemName + " in position " + position + " and rotation " + itemRotations[i]);
		}

		DestroyImmediate(placeholder);
	}

	Vector2 BoundingBoxAverageOfCoordenates(Vector2[] coordenates)
	{

		Vector2 averageOfLocalCoordenates = Vector2.zero;

		for (int i = 0; i < coordenates.Length; i++)
		{

			if (coordenates[i].x > averageOfLocalCoordenates.x)
			{
				averageOfLocalCoordenates.x = coordenates[i].x;
			}
			if (coordenates[i].y > averageOfLocalCoordenates.y)
			{
				averageOfLocalCoordenates.y = coordenates[i].y;
			}
		}

		averageOfLocalCoordenates *= 0.5f;

		return averageOfLocalCoordenates;
	}

	Vector2[] GlobalizeCoordenates(Vector2[] localCoordenates, Vector2 positionIndex){
		Vector2[] coordenates = new Vector2[ localCoordenates.Length ];
		
		for(int i=0; i<localCoordenates.Length; i++){
			coordenates[i] = localCoordenates[i] + positionIndex;
		}
		return coordenates;
	}

	Vector2[] RotateMatrixAngle(Vector2[] vector, int angle)
	{
		//Only by 90 degrees and max 3 times
		if (angle < 0)
		{
			angle = 4 - (angle % 360) / 90;
		}
		else
		{
			angle = -(angle % 360) / 90;
		}

		if (angle == 4)
			return vector;

		return RotateMatrixTimes(vector, angle);
	}
	Vector2[] RotateMatrixTimes(Vector2[] vector, int times) {

		Vector2[] vectorOut = new Vector2[vector.Length];
		vector.CopyTo(vectorOut, 0);

		for (int rotateTimes = 0; rotateTimes< times; rotateTimes++) {
			float xMax = 0;

			ArrayDebuger(vectorOut, "vectorOut before matrix rotation: ");

			//Mirror on 45 degrees
			for (int i = 0; i < vectorOut.Length; i++)
			{
				float xTemp = vectorOut[i].x;
				vectorOut[i].x = vectorOut[i].y;
				vectorOut[i].y = xTemp;

				//A reference to mirror the Y axis
				if (vectorOut[i].x > xMax)
				{
					xMax = vectorOut[i].x;
				}
			}

			//Mirror on Y axis
			for (int i = 0; i < vectorOut.Length; i++)
			{
				vectorOut[i].x = xMax - vectorOut[i].x;
			}

			ArrayDebuger(vectorOut, "vectorOut  after matrix rotation: ");
		}

		Debuger("Rotated vector " + times + " times");
		return vectorOut;
	}


	[ContextMenu("Reset Items")]
	void ResetItems(){
		if(instances== null)
			return;
		
		if(instances.Length <= 0)
			return;
		
		foreach(var instance in instances){
			DestroyImmediate(instance.gameObject);
		}

		instances = new Transform[0];
		Debuger("Deleted " + items.Length + " items in scene");
	}
	
	
	
	[System.Serializable]
	public class Items {
		[HideInInspector] public string itemName;
		public GameObject graphics;
		[HideInInspector] public Vector2[] localCoordenates;

		[HideInInspector] public float[] localCoordenatesX;
		[HideInInspector] public float[] localCoordenatesY;

		public void ReplaceInspectorData()
		{
			ReplaceLocalCoordenates();
		}

		public static Items CreateFromJSON(string jsonString)
		{
			return JsonUtility.FromJson<Items>(jsonString);
		}
		void ReplaceLocalCoordenates()
        {
			
			localCoordenates = new Vector2[localCoordenatesX.Length];
			for (int i = 0; i < localCoordenatesX.Length; i++)
			{
				localCoordenates[i].x = localCoordenatesX[i];
				localCoordenates[i].y = localCoordenatesY[i];
			}
		}
	}

	public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }
	void ArrayDebuger(Vector2[] vectorArray, string text="ArrayDebuger") { if (showDebugs) for (int i=0; i<vectorArray.Length; i++) Debug.Log(text + " ["+i+"] " + vectorArray[i]); }
}

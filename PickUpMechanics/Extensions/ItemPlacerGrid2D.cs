using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(GridBuilder2D))]
public class ItemPlacerGrid2D : MonoBehaviour
{
	
	public Items[] items;
	GridBuilder2D gridBuilder;
	Transform[] instances;
	
	//TEST
	public int[] itemInstructionIndexes = {0};
	public Vector2[] itemPositions = {Vector2.zero};
	public int[] itemRotations = {0};


    public void Setup(Items[] _items, int[] _itemIndexes, Vector2[] _positionIndexes, int[] _itemRotations)
    {
		items = _items;
		itemInstructionIndexes = _itemIndexes;
		itemPositions = _positionIndexes;
		itemRotations = _itemRotations;
	}

    [ContextMenu("Place Items")]
	public void PlaceItems(){

		//ResetItems();
		instances = new Transform[ itemInstructionIndexes.Length ];
		
		var placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
		placeholder.name = "Original Item";
		
		gridBuilder = GetComponent<GridBuilder2D>();
		
		for(int i=0; i<itemInstructionIndexes.Length; i++){
			var itemIndex = itemInstructionIndexes[i];
			var item = items[ itemIndex ];
			var graphics = item.graphics;


			if(graphics == null){
				Debuger("No graphics to use as item. Will use a placeholder");
				graphics = placeholder;
			}

			//Only by 90 degrees and max 3 times
			int rotationTimes = (int)(itemRotations[i] / 90) % 4;
			int rotationAngleClamp = rotationTimes * 90;

			//Rotate and Globalize coordenates
			Vector2 sum = Vector2.zero;
			Vector2[] localCoordenates = RotateMatrixTimes(item.localCoordenates, rotationTimes);
			Vector2[] globalCoordenates = GlobalizeCoordenatesAndFindAverage(localCoordenates, itemPositions[i], ref sum);

			//average local position and globalize to array of containers
			//then globalize to world coordenates adding holderContainers.position
			Vector3 position = sum * 0.5f + itemPositions[i];
			position.z = position.y;
			position.y = 0;
			position += gridBuilder.instances[0, 0].transform.position;

			int xIndex = (int)itemPositions[i].x;
			int yIndex = (int)itemPositions[i].y;

			//Apply transforms and properties
			Container container = gridBuilder.instances[xIndex, yIndex];
			GameObject itemInstance = Instantiate(graphics, position, Quaternion.Euler(0, rotationAngleClamp, 0) );
			itemInstance.name = item.itemName;
			itemInstance.AddComponent<Pickupable>();

			//Setup components and internal logic
			Pickupable instanceComponent = itemInstance.GetComponent<Pickupable>();

			instanceComponent.myName = item.itemName;
			instanceComponent.SetCoordenates( globalCoordenates );
			instanceComponent.SetOccupancy( container );
			container.SetOccupancy( instanceComponent );

			//Save as a reference
			instances[i] = itemInstance.transform;
			Debuger("Placed " + item.itemName + " in position " + position + " and rotation " + itemRotations[i]);
		}

		DestroyImmediate(placeholder);
	}


	
	Vector2[] GlobalizeCoordenatesAndFindAverage(Vector2[] localCoordenates, Vector2 positionIndex, ref Vector2 sum){
		Vector2[] coordenates = new Vector2[ localCoordenates.Length ];
		
		for(int i=0; i<localCoordenates.Length; i++){
			coordenates[i] = localCoordenates[i] + positionIndex;
			sum += localCoordenates[i];
		}
			
		return coordenates;
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
		public string itemName;
		public GameObject graphics;
		public Vector2[] localCoordenates;
	}

	public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }
	void ArrayDebuger(Vector2[] vectorArray, string text="ArrayDebuger") { if (showDebugs) for (int i=0; i<vectorArray.Length; i++) Debug.Log(text + " ["+i+"] " + vectorArray[i]); }
}

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
	public int[] itemIndexes = {0};
	public Vector2[] positionIndexes = {Vector2.zero};
	public int[] itemRotations = {0};


    public void Setup(Items[] _items)
    {
		items = _items;
	}

    [ContextMenu("Place Items")]
	public void PlaceItems(){

		//ResetItems();
		instances = new Transform[ itemIndexes.Length ];
		
		var placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
		placeholder.name = "Original Item";
		
		gridBuilder = GetComponent<GridBuilder2D>();
		
		for(int i=0; i<itemIndexes.Length; i++){
			var itemIndex = itemIndexes[i];
			var item = items[ itemIndex ];
			var graphics = item.graphics;
			
			if(graphics == null){
				Debuger("No graphics to use as item. Will use a placeholder");
				graphics = placeholder;
			}
			
			Vector2 sum = Vector2.zero;
			Vector2[] globalCoordenates = GlobalizeCoordenatesAndFindAverage( item.localCoordenates, positionIndexes[i], ref sum);
			globalCoordenates = RotateMatrix( globalCoordenates );
			
			int xIndex = (int)positionIndexes[i].x;
			int yIndex = (int)positionIndexes[i].y;
			
			Vector3 position = sum * 0.5f + positionIndexes[i];
			position.z = position.y;
			position.y = 0;
			position += gridBuilder.instances[0,0].position;
			
			Transform parent = gridBuilder.instances[xIndex, yIndex];
			GameObject instance = Instantiate(graphics, position, Quaternion.Euler(0, itemRotations[i], 0) );
			instance.name = item.itemName;
			instance.AddComponent<Pickupable>();

			Pickupable instanceComponent = instance.GetComponent<Pickupable>();
			Container container = parent.GetComponent<Container>();
			
			instanceComponent.myName = item.itemName;
			instanceComponent.SetCoordenates( globalCoordenates );
			instanceComponent.SetOccupancy( container );
			container.SetOccupancy( instanceComponent );
			
			instances[i] = instance.transform;
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
	
	Vector2[] RotateMatrix(Vector2[] matrix){
		return matrix;
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
		public Vector2[] localCoordenates;
		public GameObject graphics;
	}

	public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }

}

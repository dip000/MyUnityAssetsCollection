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

    public void Setup(Items[] _items)
    {
		items = _items;
	}

    [ContextMenu("Place Items")]
	public void PlaceItems(){
		ResetItems();
		instances = new Transform[ items.Length ];
		
		var placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
		placeholder.AddComponent<Pickupable>();
		placeholder.name = "Original Item";
		
		gridBuilder = GetComponent<GridBuilder2D>();
		
		for(int i=0; i<items.Length; i++){
			
			var graphics = items[i].graphics;
			
			if(items[i].graphics == null){
				Debuger("No graphics to use as item. Will use a placeholder");
				graphics = placeholder;
			}
			
			int xlocation = (int)items[i].location.x;
			int ylocation = (int)items[i].location.y;

			Transform parent = gridBuilder.instances[xlocation, ylocation];
			//parents[i] = gridBuilder.instances[xlocation, ylocation];
			GameObject instance = Instantiate(graphics, parent);
			instance.name = items[i].name;
			
			Pickupable instanceComponent = instance.GetComponent<Pickupable>();
			instanceComponent.myName = items[i].name;
			
			Vector3[] coordenates = new Vector3[1];
			coordenates[0] = (Vector3)items[i].location;
			instanceComponent.SetCoordenates( coordenates );
			
			instances[i] = instance.transform;
			
			Debuger("Placed " + items[i].name + " in " + items[i].location);
		}
		
		
		placeholder.SetActive(false);
	}
	
	[ContextMenu("Reset Items")]
	void ResetItems(){
		if(instances== null)
			return;
		
		foreach(var instance in instances){
			DestroyImmediate(instance);
		}

		instances = new Transform[0];
		Debuger("Deleted " + items.Length + " items in scene");
	}
	
	[System.Serializable]
	public class Items {
		public string name;
		public GameObject graphics;
		public Vector2 location;

		//public string itemName;
		//public bool[,] shape;
	}

	public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }

}

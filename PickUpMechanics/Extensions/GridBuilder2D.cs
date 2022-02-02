using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class GridBuilder2D : MonoBehaviour
{
	public Vector2 containers = new Vector2(3,2);
	public Vector2 containersSize = new Vector2(0.8f,0.8f);
	public float margin = 0.2f;
	
	public GameObject containerGraphics;
	public GameObject holderOfContainers;

	public Container[,] instances {get; private set;}

	public GameObject graphicsParent;
	
    private void Awake()
    {
	}

	public void Setup(Vector2 _containers, Vector2 _containersSize, float _margin)
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
		
		float detectionRadius = (containersSize.x < containersSize.y) ? containersSize.x : containersSize.y;
		detectionRadius *= 0.5f;
		graphicsParent.AddComponent<ArrayHolderRegister>();
		graphicsParent.AddComponent<AditionalConditionsForPickUpMechanics>();

		for (int i=0; i<containers.x; i++){			
			for(int j=0; j<containers.y; j++){
				GameObject instance = Instantiate(graphics, graphicsParent.transform);
				instance.transform.localScale = new Vector3( containersSize.x, 1, containersSize.y );
				instance.name = "Container " + i + "-" + j;
				
				Vector3 objectPosition = Vector3.zero;
				objectPosition.x = i * (containersSize.x + margin);
				objectPosition.z = j * (containersSize.y + margin);
				
				instance.transform.position = objectPosition;
				
				Container instanceComponent = instance.GetComponent<Container>();
				instanceComponent.coordenates = new Vector2(i, j);
				
				instances[i, j] = instanceComponent;
			}
		}

		graphicsParent.GetComponent<ArrayHolderRegister>().RegisterContainers(instances);

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
	
	public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }

}

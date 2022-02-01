using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class GridBuilder2D : MonoBehaviour
{
	public Vector2 containers = new Vector2(2,2);
	public Vector2 containersSize = new Vector2(1,1);
	public float margin = 0.0f;
	
	public GameObject containerGraphics;
	public GameObject holderOfContainers;

	public Transform[,] instances {get; private set;}
	
	
	GameObject graphics;
	GameObject graphicsParent;
    [ContextMenu("Build Grid")]
    private void Awake()
    {
	}

	public void Setup(Vector2 _containers, Vector2 _containersSize, float _margin)
    {
		containers = _containers;
		containersSize = _containersSize;
		margin = _margin;
	}

	public void BuildGrid(){
		ResetGrid();
		
		graphics = containerGraphics;
		graphicsParent = holderOfContainers;
		instances = new Transform[ (int)containers.x,(int)containers.y ];
		
		if(containerGraphics == null){
			Debuger("No graphics to use as containers. Will use placeholders");
			graphics = GameObject.CreatePrimitive(PrimitiveType.Cube);
			graphics.AddComponent<Container>();
			graphics.name = "Original Container";
		}
		
		if(holderOfContainers == null){
			Debuger("No holderContainer. Will make one at World Origin");
			graphicsParent = new GameObject("Holder Of Containers");
		}
		
		float detectionRadius = (containersSize.x < containersSize.y) ? containersSize.x : containersSize.y;
		detectionRadius *= 0.5f;
		
		for(int i=0; i<containers.x; i++){			
			for(int j=0; j<containers.y; j++){
				GameObject instance = Instantiate(graphics, graphicsParent.transform);
				instance.transform.localScale = new Vector3( containersSize.x, 1, containersSize.y );
				instance.name = "Container " + i + "-" + j;
				
				Vector3 objectPosition = Vector3.zero;
				objectPosition.x = i * (containersSize.x + margin);
				objectPosition.z = j * (containersSize.y + margin);
				
				instance.transform.position = objectPosition;
				
				Container instanceComponent = instance.GetComponent<Container>();
				instanceComponent.coordenates = new Vector3(i, j, 0);
				instanceComponent.detectionRadius = detectionRadius;
				
				instances[i, j] = instance.transform;
			}
		}
		
		
		graphics.SetActive(false);
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

		DestroyImmediate(graphics);
		DestroyImmediate(graphicsParent);
		instances = new Transform[0,0];
	}
	
	public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }

}

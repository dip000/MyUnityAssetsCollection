using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : MonoBehaviour
{
	
    public string myName = "ObjectX";
    public Vector2[] shape { get; private set; }
    public Container myContainer { get; private set; }


	public void ResetOccupancy(){
		myContainer = null;
	}

	public void SetOccupancy(Container _myContainer){
		myContainer = _myContainer;
		//print( "_myContainer.transform.name:" + _myContainer.transform.name );
	}

	public void SetShape(Vector2[] _shape)
	{
		shape = _shape;
		//print( "_shape: " + _shape[0] );
	}

	public void AcomodateInMyContainer()
	{
		transform.position = myContainer.transform.position;
		//transform.parent = myContainer.transform;
	}
}

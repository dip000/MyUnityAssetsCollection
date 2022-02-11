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
	}
	
	public void SetShape(Vector2[] _shape)
	{
		shape = _shape;
	}

	public void AcomodateInMyContainer()
	{
		transform.position = myContainer.transform.position;
		//transform.parent = myContainer.transform;
	}
}

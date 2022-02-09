using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : MonoBehaviour
{
	
    public string myName = "ObjectX";

	//Global coordenates of the current position being occupied
    public Vector2[] coordenates { get; private set; }

	//Position of this item where it was dropped
	//ArrayHolderRegister uses the lowest point in X and Y of coordenates variable
	public Vector2 coordenateIndex { get; private set; }
    public Container myContainer { get; private set; }

	public void ResetOccupancy(){
		myContainer = null;
	}
	
	public void SetOccupancy(Container _myContainer){
		myContainer = _myContainer;
	}
	
	public void SetCoordenates(Vector2[] _coordenates, Vector2 _coordenateIndex)
	{
		coordenates = _coordenates;
		coordenateIndex = _coordenateIndex;
	}
	public void SetCoordenates(Vector2[] _coordenates)
	{
		coordenates = _coordenates;
	}

	public void AcomodateInMyContainer()
	{
		transform.position = myContainer.transform.position;
		//transform.parent = myContainer.transform;
	}
}

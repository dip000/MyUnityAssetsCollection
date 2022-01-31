using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : MonoBehaviour
{
	
    public string myName = "ObjectX";
    public Vector3[] coordenates { get; private set; }
    public Container myContainer { get; private set; }
	
	public void ResetOccupancy(){
		myContainer = null;
	}
	
	public void SetOccupancy(Container _myContainer){
		myContainer = _myContainer;
	}
	
	public void SetCoordenates(Vector3[] _coordenates){
		coordenates = _coordenates;
	}
}

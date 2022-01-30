using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : MonoBehaviour
{
	
    public string myName = "ObjectX";
    public Vector3[] coordenates;
    public Container myContainer { get; private set; }
	
	
	//(Optional) INFORMATION FOR ARRAY HOLDERS
	
	//1's and 0's that reprecents its shape in a 2D local space
	//Its size is represented as a square bounding box so it can be array-rotated easily
	public bool[,] shape;
	
	//offset position over an array holder
	public Vector3 indexPosition;
	
	public void ResetOccupancy(){
		myContainer = null;
	}
	
	public void SetOccupancy(Container _myContainer){
		myContainer = _myContainer;
	}
}

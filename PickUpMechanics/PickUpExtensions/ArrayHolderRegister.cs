using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ArrayHolderRegister : MonoBehaviour {

	public bool[,] occupancyMap;

	public void Setup(Vector2 size)
    {
		occupancyMap = new bool[(int)size.x, (int)size.y];
		Debuger("Containers Setted up");
	}

	public void UpdateCoordinatesInOccupancyMap(Vector2[] coordinates, bool state){
		
		try{
			string debugString = "Updated "+ coordinates.Length + " coordinates in map: ";
			for(int i=0; i<coordinates.Length; i++){
				occupancyMap[(int)coordinates[i].x, (int)coordinates[i].y] = state;
				debugString += ", " + coordinates[i].ToString();
			}
			Debuger( debugString );
		} catch{
			Debug.LogWarning("ARRAY HOLDER REGISTER. Can't register coordenate outside of bounds. ArrayHolderRegister.cs does not manage item Drop conditions. Refer to the script that does to know why it allowed this to happen, like AditionalConditionsForPickupMechnics.cs or the like", this);
		}
	}

	//------------------------ DEBUGERS -------------------------------------
	public bool showDebugs=true; void Debuger(string text) { if (showDebugs) Debug.Log(text); }
	void ArrayDebuger(Vector2[] vectorArray, string text="ArrayDebuger") {
		if( showDebugs )
		{
			var debugString = text;
			for( int i = 0; i < vectorArray.Length; i++ )
				debugString = "," + vectorArray[i];
			Debug.Log(debugString);
		}
	}

}
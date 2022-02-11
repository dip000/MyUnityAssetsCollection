using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObjectOnHand : MonoBehaviour
{
	ArrayHolderRegister arrayHolderRegister;
	bool enableRotations = false;
	
	public void StartObjectRotations(ArrayHolderRegister _arrayHolderRegister){
		arrayHolderRegister = _arrayHolderRegister;
		enableRotations = true;
	}

	// Update is called once per frame
	void Update()
	{
		if(enableRotations == false){
			return;
		}
		
		if(PickUpMechanics.hasObjectOnHand == false)
        {
			return;
        }

		float delta = Input.GetAxis("Mouse ScrollWheel") * 10.0f;

		int timesToRotate = 1;
		int directionSign = -1;

		if(delta > 0.0f)
		{
			timesToRotate = 3;
			directionSign = 1;
		}

		if (delta != 0)
		{
			PickUpMechanics.handObject.transform.Rotate(Vector3.up, directionSign*90);
			RotateItem(timesToRotate);
		}

	}



	void RotateItem(int rotation)
    {
		//Rotate and Globalize coordenates
		//arrayHolderRegister.localCoordenates = RotateMatrixTimes(arrayHolderRegister.localCoordenates, rotation);
	}

	Vector2[] RotateMatrixTimes(Vector2[] vector, int times)
	{

		Vector2[] vectorOut = new Vector2[vector.Length];
		vector.CopyTo(vectorOut, 0);

		for (int rotateTimes = 0; rotateTimes < times; rotateTimes++)
		{
			float xMax = 0;

			//Mirror on 45 degrees
			for (int i = 0; i < vectorOut.Length; i++)
			{
				float xTemp = vectorOut[i].x;
				vectorOut[i].x = vectorOut[i].y;
				vectorOut[i].y = xTemp;

				//A reference to mirror the Y axis
				if (vectorOut[i].x > xMax)
				{
					xMax = vectorOut[i].x;
				}
			}

			//Mirror on Y axis
			for (int i = 0; i < vectorOut.Length; i++)
			{
				vectorOut[i].x = xMax - vectorOut[i].x;
			}

		}

		Debuger("Rotated vector " + times + " times");
		return vectorOut;
	}
	public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }
}

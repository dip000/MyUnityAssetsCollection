using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObjectOnHand : MonoBehaviour
{
	public bool enableRotations = false;
	
	public void StartObjectRotations(){
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

		int directionSign = -1;

		if(delta > 0.0f)
		{
			directionSign = 1;
		}

		if (delta != 0)
		{
			PickUpMechanics.handObject.transform.Rotate(Vector3.up, directionSign*90);
		}

	}
	public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }
}

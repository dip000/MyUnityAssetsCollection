using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObjectOnHand : MonoBehaviour
{
    public float scrollComparator = 1.0f;
	public float scrollDeltaAcumulated;

    // Update is called once per frame
    void Update()
    {
        if(PickUpMechanics.hasObjectOnHand){
			if(Input.mouseScrollDelta.y != 0){
				Debuger("Mouse scroll started with value:" + scrollDeltaAcumulated);
				scrollDeltaAcumulated = Input.mouseScrollDelta.y;
				StartCoroutine( ScrollMouseMotion() );
			}
		}
		else{
			scrollDeltaAcumulated = 0;
		}
    }
	
	int rotation;
	
	IEnumerator ScrollMouseMotion(){
		if(Input.mouseScrollDelta.y == 0){
			Debuger("Mouse scroll stopped with value:" + scrollDeltaAcumulated);
			yield break;
		}
		
		yield return new WaitForSeconds(0.05f);
		
		scrollDeltaAcumulated += Input.mouseScrollDelta.y;
		
		if(Mathf.Abs(scrollDeltaAcumulated) >= scrollComparator){
			scrollDeltaAcumulated -= scrollComparator;
			
			int rotationAngleClamp = (++rotation * 90)%360;
			PickUpMechanics.handObject.transform.rotation = Quaternion.Euler(0, rotationAngleClamp, 0);
			
			Debuger("Scroll accumulated to the next change");
		}
	}
    public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }
}

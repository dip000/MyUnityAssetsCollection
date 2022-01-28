using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{

    [Header("DO NOT let it overlap with another container!")]
    public float detectionRadius = 1.0f;

    [HideInInspector] public int fill = 0;
    [HideInInspector] public Pickupable objectInside;
    [HideInInspector] public bool isBlocked;

    // Start is called before the first frame update
    void Start()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);

        for (int i=0; i< hitColliders.Length; i++)
        {
            Pickupable testObjectInside = hitColliders[i].GetComponent<Pickupable>();
            if(testObjectInside != null)
            {
                fill = 1;

                objectInside = testObjectInside;
                objectInside.myContainer = this;

                objectInside.transform.parent = transform;

                objectInside.transform.position = transform.position;

                Debuger("" + transform.name + "Has auto detected pickupable" + objectInside.transform.name);
                break;
            }
        }
    }

    public void Reset()
    {
        fill = 0;
        objectInside = null;
    }

    [Header("Check to visualize detection radius")]
    public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }

    void OnDrawGizmosSelected()
    {
        if (showDebugs)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupableAutoInserter : MonoBehaviour
{
	public static Container[] containers;
	public float detectionRadius = 1.0f;

	void Start()
	{
		//1. Get all Containers in scene
		containers = FindObjectsOfType<Container>();
		if (containers.Length <= 0)
		{
			Debug.LogWarning("PICKUPABLE AUTO INSERTER. No containers in scene to auto insert");
		}
		Debuger("Found " + containers.Length + " containers in scene");

		//2. Fill container's data
		for(int i=0; i<containers.Length; i++)
        {
			InitializeValues(containers[i]);
		}

	}

	void InitializeValues(Container container)
	{
		Pickupable objectInside = AutoFindItemNearby(container);

		if (objectInside == null)
		{
			container.ResetOccupancy();
		}
		else
		{
			//Link each other references for PickUpMechanics.cs
			container.SetOccupancy(objectInside);
			objectInside.SetOccupancy(container);

			//Ideally, objects with non unitarean shapes should't be placed like this
			if (container.yieldControlToExternal == false)
			{
				objectInside.AcomodateInMyContainer();
			}
		}
	}

	Pickupable AutoFindItemNearby(Container container)
	{
		//Any Pickupable in range of this sphere, is autodetected as being inside container
		Collider[] hitColliders = Physics.OverlapSphere(container.transform.position, detectionRadius);

		for (int i = 0; i < hitColliders.Length; i++)
		{
			Pickupable testObjectInside = hitColliders[i].GetComponent<Pickupable>();
			if (testObjectInside != null)
			{
				Debuger("" + container.name + " Has auto detected " + testObjectInside.transform.name);
				return testObjectInside;
			}
		}

		return null;
	}


	[Header("Check to visualize detection radius")]
	public bool showDebugs; void Debuger(string text) { if (showDebugs) Debug.Log(text); }

	void OnDrawGizmosSelected()
	{
		if (showDebugs)
		{
			Container[] containers = FindObjectsOfType<Container>();
			for (int i = 0; i < containers.Length; i++)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(containers[i].transform.position, detectionRadius);
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InstructionInterpreterForPickupMechanics : MonoBehaviour
{
	Dictionary<Pickupable, Vector2> volumesCenter = new Dictionary<Pickupable, Vector2>();

	void Awake(){
		PickUpMechanics.OnPickUp += OnPickUp;
		PickUpMechanics.OnDrop += OnDrop;
	}

	HandItemPrecalculations handItemPrecalculations;
	public class HandItemPrecalculations {
		Dictionary<int, Vector2[]> shapeRotations;
		Dictionary<int, Vector2> shapeVolumeCenters;
		const int totalRotations = 4;

        public HandItemPrecalculations( Pickupable item, Container container)
        {
			var itemRotation = 0;
			for(int i=0; i< totalRotations; i++ )
            {
				itemRotation += 90;
				shapeRotations[itemRotation] = Vector2Calculations.RotateMatrixAngle( item.shape, itemRotation );
			}

			//Coordenates where the shape is placed
			var volumeCenter = Vector2Calculations.RoundVector( Vector2Calculations.VolumeCenter( shapeRotated ) );
			var placementPosition = container.coordenates - volumeCenter;
			var globalCoordenates = Vector2Calculations.Globalize( shapeRotated, placementPosition );

			//The register assigned for the target container
			var register = PickUpMechanics.targetContainer.containerRegister;

			//Apply transforms and properties
			var worldSpacePosition = Vector2Calculations.GridSpacePosition( shapeRotated, placementPosition ) + register.position;
		}

		public Vector2[] Rotation(int angle){
			return shapeRotations[angle];
		}
		public Vector2 Position( Vector2 position ){
			return position;
		}
	}

	//------------------------ REGISTERED EVENTS -------------------------------------
	void OnPickUp()
	{
		//Look up variables
		var item = PickUpMechanics.targetPickupable;
		var container = PickUpMechanics.targetContainer;

		//Get and update coordenates to register
		var globalCoordenates = Vector2Calculations.Globalize( item.shape, container.coordenates );

		//Calculate Volume Center if it is the first time this item was picked
		handItemPrecalculations = new HandItemPrecalculations( item , container );

		//Router data to the corresponding register
		var register = PickUpMechanics.targetContainer.containerRegister;
		if( register.TryGetComponent( out ArrayHolderRegister registerComponent ))
			registerComponent.UpdateCoordenatesInOccupancyMap( globalCoordenates, PickUpMechanics.free );
		else
			Debug.LogWarning("INSTRUCTION INTERPRETER. Register " + register.name + " Must have a ArrayHolderRegister to register data" );
	}

	void OnDrop(){
		//Look up variables. handObject is not nulled on drop, instead is marked as "hasObjectOnHand = false"
		Pickupable item = PickUpMechanics.handObject;
		Container container = PickUpMechanics.targetContainer;

		//The register assigned for the target container
		var register = PickUpMechanics.targetContainer.containerRegister;

		//Coordenates where the shape is placed
		var itemRotation = (int)item.transform.eulerAngles.y;
		var shapeRotated = Vector2Calculations.RotateMatrixAngle( item.shape, itemRotation );
		var volumeCenter = Vector2Calculations.RoundVector( Vector2Calculations.VolumeCenter( shapeRotated ) );
		var placementPosition = container.coordenates - volumeCenter;
		var globalCoordenates = Vector2Calculations.Globalize( shapeRotated, placementPosition );

		//Apply transforms and properties
		var worldSpacePosition = Vector2Calculations.GridSpacePosition( shapeRotated, placementPosition ) + register.position;

		//Apply transforms and properties
		item.transform.position = worldSpacePosition;
		item.transform.parent = null;

		//Router data to the corresponding register
		if( register.TryGetComponent( out ArrayHolderRegister registerComponent ) )
			registerComponent.UpdateCoordenatesInOccupancyMap( globalCoordenates, PickUpMechanics.occupied );
		else
			Debug.LogWarning( "INSTRUCTION INTERPRETER. Register " + register.name + " Must have a ArrayHolderRegister to register data" );

	}
	public bool showDebugs = false;  void Debuger( string text ) { if( showDebugs ) Debug.Log( text ); }
	void ArrayDebuger( Vector2[] vectorArray, string text = "ArrayDebuger" ){
		if( showDebugs ){
			var debugString = text;
			foreach( var vector in vectorArray ) debugString += ", " + vector;
			Debug.Log( debugString );
		}
	}
}
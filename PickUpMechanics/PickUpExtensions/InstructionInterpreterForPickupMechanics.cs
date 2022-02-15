using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InstructionInterpreterForPickupMechanics : MonoBehaviour
{
	Dictionary<Pickupable, ItemPrecalculations> itemsPrecalculations = new Dictionary<Pickupable, ItemPrecalculations>();

	void Awake(){			
		PickUpMechanics.OnPickUp += OnPickUp;
		PickUpMechanics.OnDrop += OnDrop;
	}
	
////////////////////////// ADITIONAL CONDITIONS //////////////////////////////////////////////////

	public Vector2[] CoordenatesOfTarget(Pickupable item){
		Container container = PickUpMechanics.targetContainer;
		var angle = (int)item.transform.eulerAngles.y;
		var conditions = PickUpMechanics.targetContainer.containerRegister;

		if( itemsPrecalculations.ContainsKey(item) == false){
			itemsPrecalculations[item] = new ItemPrecalculations( item );
			Debuger("Precalculated new item: " + item.myName);
		}
		
		return Vector2Calculations.Globalize( itemsPrecalculations[item].ShapeByAngle(angle), container.coordenates );
	}
	
//////////////////////////////////////////////////////////////////////////////////////////////////

	

//////////////////////////////// PICKUP EVENTS //////////////////////////////////////////////
	void OnPickUp()
	{
		//Look up variables
		var item = PickUpMechanics.targetPickupable;
		var container = PickUpMechanics.targetContainer;

		//Precalculate everithing if it is the first time this item was picked up
		if( itemsPrecalculations.ContainsKey(item) == false){
			itemsPrecalculations[item] = new ItemPrecalculations( item );
			Debuger("Precalculated new item: " + item.myName);
		}

		//Get and update coordenates to register
		var itemRotation = (int)item.transform.eulerAngles.y;
		var globalCoordenates = Vector2Calculations.Globalize( itemsPrecalculations[item].ShapeByAngle(itemRotation), container.coordenates );
		ArrayDebuger(globalCoordenates, "Bug of first pick here! globalCoordenates: ");

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
		ItemPrecalculations itemPrecalculations = itemsPrecalculations[item];

		//The register assigned for the target container
		var register = PickUpMechanics.targetContainer.containerRegister;
		var itemRotation = (int)item.transform.eulerAngles.y;

		//Spatial locations using precalculated variables
		var globalCoordenates = Vector2Calculations.Globalize( itemPrecalculations.ShapeByAngle(itemRotation), container.coordenates );
		var gridSpacePosition = itemPrecalculations.CenterByAngle(itemRotation) + container.coordenates;
		var worldSpacePosition = new Vector3( gridSpacePosition.x, 0, gridSpacePosition.y ) + register.position;

		//Apply transforms and properties
		item.transform.position = worldSpacePosition;
		item.transform.parent = null;

		//Router data to the corresponding register
		if( register.TryGetComponent( out ArrayHolderRegister registerComponent ) )
			registerComponent.UpdateCoordenatesInOccupancyMap( globalCoordenates, PickUpMechanics.occupied );
		else
			Debug.LogWarning( "INSTRUCTION INTERPRETER. Register " + register.name + " Must have a ArrayHolderRegister to register data" );

	}


//////////////////////////////////////////////////////////////////////////////////////////////////

///////////////////////////////// CALCULATIONS ///////////////////////////////////////////////////
	
	public class ItemPrecalculations {
		Dictionary<int, Vector2[]> shapeRotations;
		Dictionary<int, Vector2> shapeCenters;
		bool isUnitarian = false;

        public ItemPrecalculations( Pickupable item )
        {
			if(item.shape.Length == 1){
				isUnitarian = true;
				return;
			}
			
			shapeRotations = new Dictionary<int, Vector2[]>();
			shapeCenters = new Dictionary<int, Vector2>();
			
			//Same bounding box center for all rotations
			var shapeBoundsCenter = Vector2Calculations.BoundingBoxCenterOfCoordenates( item.shape );
			for(int rot=0; rot<360; rot+=90 )
            {
				//Saves all rotations so it doesn't have to recalculate on real time
				//This places the item from volume center instead of left-bottom pivot
				//Volume center is fussed with the shape in a displacement
				var shapeRotated = Vector2Calculations.RotateMatrixAngle( item.shape, rot );
				var shapeVolumeCenter = Vector2Calculations.RoundVector( Vector2Calculations.VolumeCenter( shapeRotated ) );
				var shapeDisplaced = Vector2Calculations.ReferenceCoordenates(shapeRotated, -shapeVolumeCenter);
				shapeRotations[rot] = shapeDisplaced;
				
				shapeCenters[rot] = shapeBoundsCenter - shapeVolumeCenter;

			}
		}

		public Vector2[] ShapeByAngle(int angle){
			if(isUnitarian)
				return new Vector2[1] {Vector2.zero};
			
			if(angle < 0)
				angle += 360;
			return shapeRotations[angle];
		}
		public Vector2 CenterByAngle(int angle){
			if(isUnitarian)
				return Vector2.zero;
			
			if(angle < 0)
				angle += 360;
			return shapeCenters[angle];
		}
	}
//////////////////////////////////////////////////////////////////////////////////////////////////
	
	
//////////////////////////////////////// DEBUGS //////////////////////////////////////////////////
	public bool showDebugs = false;  void Debuger( string text ) { if( showDebugs ) Debug.Log( text ); }
	void ArrayDebuger( Vector2[] vectorArray, string text = "ArrayDebuger" ){
		if( showDebugs ){
			var debugString = text;
			foreach( var vector in vectorArray ) debugString += ", " + vector;
			Debug.Log( debugString );
		}
	}
}
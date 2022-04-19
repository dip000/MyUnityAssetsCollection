using System.Collections;
using UnityEngine;
using System.Linq;


//Executes before all other scripts
[DefaultExecutionOrder( -100 )]
public class RayCaster : MonoBehaviour {
	static public RayCaster instance;

	//Look up variables
	public static bool castRays = true;
	public static RaycastHit[] objectsHit = new RaycastHit[0];

	//For runtime internal (not on inspector)
	static bool evaluateOnChange;
	static Config castRaysWhen;
	static LayerMask detectionLayerMask;
	static LayerMask dynamicBlockingMask;
	static float maxDistance;

	//For runtime external
	public enum Config { asked, always };

	//For Inspector set up (not on runtime)
	public bool _evaluateOnChange = true;
	public Config _castRaysWhen = Config.always;
	public LayerMask _detectionLayerMask = Physics.DefaultRaycastLayers;
	public LayerMask _dynamicBlockingMask = 0;
	public float _maxDistance = 100;
	public bool _showDebugs = false;

	void Awake() {
		instance = this;
	}

	void Start()
	{
		//Full setup. To change it, use: LayersSettings() and CastRaysOncePerFrameSettings()
		//Must be in order: SetupValues, SeccurityStartChecks, CastRaysOncePerFrameSetup
		SetupValues();
		SeccurityStartChecks();
		StartCoroutine( EndOfFrame() );
	}

	private void Update()
	{
		if( castRaysWhen == Config.always )
		{
			CastRaysOncePerFrame();
			ArrayDebuger( objectsHit, "objectsHit name:" );
		}
	}

	void SetupValues() {
		showDebugs = _showDebugs;
		maxDistance = _maxDistance;
		detectionLayerMask = _detectionLayerMask;
		dynamicBlockingMask = _dynamicBlockingMask;
		castRaysWhen = _castRaysWhen;
		evaluateOnChange = _evaluateOnChange;
	}

	static void SeccurityStartChecks() {
		//Seccurity check 1. No mixed blocking mask
		float log2Mask = Mathf.Log( dynamicBlockingMask, 2 );
		if( log2Mask != (int)log2Mask && dynamicBlockingMask != 0 )
		{
			Debug.LogWarning( "RAYCASTER: dynamicBlockingMask cannot accept a mixed mask of " + dynamicBlockingMask + ". Reseted to 0" );
			dynamicBlockingMask = 0;
		}

		//Seccurity check 2. Read layers must include the blocking mask for a blocking to be detected
		detectionLayerMask |= dynamicBlockingMask;
	}

	static void CastRaysOncePerFrame()
	{
		//To avoid redoundant casts. Resets on end of frame
		if( alreadyCasted ) return;
		alreadyCasted = true;

		//Ray from mouse pointer forwards
		Vector3 mouse = Input.mousePosition;
		Ray castPoint = Camera.main.ScreenPointToRay( mouse );

		//Cast a ray to everything and always sort by distance
		objectsHit = Physics.RaycastAll( castPoint, maxDistance, detectionLayerMask ).OrderBy( h => h.distance ).ToArray();
		RayDebuger( castPoint );
		
		OnChange();
	}


	//------------------ RUNTIME FUNCTIONS --------------------------------

	static bool DetectMaskBlocking( RaycastHit hit )
	{
		//If there's an actual blocking layer
		if( dynamicBlockingMask <= 0 )
		{
			return false;
		}

		//Cheap way of dectecting an object layer. Optimized but limited to 1 blocker layer
		if( (1<<hit.transform.gameObject.layer) != dynamicBlockingMask )
		{
			return false;
		}

		Debuger( "Blocked mask: " + (int)dynamicBlockingMask + "; Of object " + hit.transform.name );
		return true;
	}


	//------------------------- DEBUGERS -----------------------------------------
	static bool showDebugs;
	static void Debuger( string text ) { if( showDebugs ) Debug.Log( text ); }
	static void RayDebuger( Ray ray ) {
		if( showDebugs )
			Debug.DrawRay( ray.origin, ray.direction * maxDistance, Color.red, 0.05f );
	}
	void ArrayDebuger( RaycastHit[] vectorArray, string text = "ArrayDebuger" )
	{
		if( showDebugs )
		{
			var debugString = text;
			foreach( var vector in vectorArray ) debugString += ", " + vector.transform.name;
			Debug.Log( debugString );
		}
	}

	//------------------------- TESTS -----------------------------------------

	static bool alreadyCasted = false;
	public static T GetFirstHitComponent<T>( bool layerBlocker = true )
	{
		CastRaysOncePerFrame();
		
		foreach( var objectHit in objectsHit )
		{
			bool componentFound = objectHit.transform.TryGetComponent( out T actualComponent );

			//If current hit has a blocker layer, stop because there's no next valid hit component
			if( layerBlocker && !componentFound )
			{
				//Spare it if it is a blocker itself
				if( DetectMaskBlocking( objectHit ) )
					return default;
			}

			if( componentFound )
			{
				return actualComponent;
			}
		}

		return default;
	}

	IEnumerator EndOfFrame()
	{
		while( true )
		{
			yield return new WaitForEndOfFrame();
			alreadyCasted = false;
		}
	}


	static public bool TryGet<T>( out T component, bool layerBlocker = true)
    {
		CastRaysOncePerFrame();
		component = default;

		foreach( var objectHit in objectsHit )
		{
			bool componentFound = objectHit.transform.TryGetComponent( out component );

			//If current hit has a blocker layer, stop because there's no next valid hit component
			if( layerBlocker && !componentFound )
			{
				//Spare it if it is a blocker itself
				if( DetectMaskBlocking( objectHit ) )
					return false;
			}

			if( componentFound )
			{
				return true;
			}
		}

		return false;
	}

	static public bool HitTransform( Transform component )
    {
		CastRaysOncePerFrame();

		foreach( var objectHit in objectsHit )
		{
			if( objectHit.transform == component )
				return true;
		}

		return false;
	}

	//------------------- TESTS ---------------------------------------------

	static RaycastHit[] previousObjectsHit = new RaycastHit[0];
    public static event System.Action OnRayCastChange;
	
	public static void OnChange(){
		
		if(evaluateOnChange == false) return;
	
		var hasChanged = false;
		
		if(previousObjectsHit.Length != objectsHit.Length){
			hasChanged = true;
		}
		else{
			for(int i=0; i<objectsHit.Length; i++){
				var hitTransform = objectsHit[i].transform;
				if( hitTransform != previousObjectsHit[i].transform ){
					hasChanged = true;
					break;
				}
			}
		}
				
		previousObjectsHit = objectsHit;
		
		if(hasChanged){
			OnRayCastChange?.Invoke();
		}
	}

}

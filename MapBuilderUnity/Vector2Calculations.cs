using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Calculations : object
{
	public static Vector2 RoundVector(Vector2 vector){
		float x = (float)System.Math.Round(vector.x);
		float y = (float)System.Math.Round(vector.y);
		return new Vector2( x, y );
	}
	
	public static Vector2 VolumeCenter(Vector2[] coordenates){
		
		if( coordenates.Length == 1 )
			return Vector3.zero;

		Vector2 volumeCenter = Vector3.zero;
		for( int i = 0; i < coordenates.Length; i++ )
		{
			volumeCenter += coordenates[i];
		}
		return volumeCenter / coordenates.Length;
	}
	
	public static Vector2 BoundingBoxCenterOfCoordenates(Vector2[] coordenates)
	{
		//Middle of point zero is itself
		if( coordenates.Length == 1 )
			return Vector2.zero;
		
		//The middle of the farthest point in X,Y is the bounding box center
		return ( BoundsOfCoordenates( coordenates ) * 0.5f );
	}
	public static Vector2 BoundsOfCoordenates( Vector2[] points )
	{
		Vector2 maxBoundsPoint = Vector3.zero;
		for( int i = 0; i < points.Length; i++ )
		{
			if( points[i].x > maxBoundsPoint.x )
				maxBoundsPoint.x = points[i].x;

			if( points[i].y > maxBoundsPoint.y )
				maxBoundsPoint.y = points[i].y;
		}
		return maxBoundsPoint;
	}

	public static Vector2[] ReferenceCoordenates(Vector2[] coordenates, Vector2 reference)
    {
		Vector2[] referenced= new Vector2[coordenates.Length];

		for( int i = 0; i < coordenates.Length; i++ ){
			referenced[i] = coordenates[i] + reference;
		}

		return referenced;
	}

	public static Vector2[] Globalize(Vector2[] localCoordenates, Vector2 positionIndex){
		return ReferenceCoordenates(localCoordenates, positionIndex);
	}

	public static Vector2[] Localize(Vector2[] globalCoordenates, Vector2 positionIndex) { 
		return ReferenceCoordenates(globalCoordenates, -positionIndex);
	}

	public static Vector2[] RotateMatrixAngle(Vector2[] vector, int angle)
	{
		//changes angle in degrees by times to rotate 90 degrees
		
		int times = (int)System.Math.Round( (angle % 360) / 90.0f );
		if (angle < 0)
			times = -times;
		else
			times = 4 - times;

		//Debug.Log( "Rotating angle: " + angle + " times: " + times );
		//Rotate 4 times 90 degrees is not rotating at all
		if( times == 0 || times == 4 )
			return vector;


		return RotateMatrixTimes(vector, times);
	}
	
	public static Vector2[] RotateMatrixTimes(Vector2[] vector, int times) {

		Vector2[] vectorOut = new Vector2[vector.Length];
		vector.CopyTo(vectorOut, 0);

		for (int rotateTimes = 0; rotateTimes< times; rotateTimes++) {
			float xMax = 0;

			//Mirror on 45 degrees
			for (int i = 0; i < vectorOut.Length; i++)
			{
				float xTemp = vectorOut[i].x;
				vectorOut[i].x = vectorOut[i].y;
				vectorOut[i].y = xTemp;

				//A reference to mirror the Y axis
				if (vectorOut[i].x > xMax)
					xMax = vectorOut[i].x;
			}

			//Mirror on Y axis
			for (int i = 0; i < vectorOut.Length; i++)
				vectorOut[i].x = xMax - vectorOut[i].x;

		}

		return vectorOut;
	}
	
	public static Vector2[] VectorizeComponents(float[] x, float[] y){
		Vector2[] vectorized = new Vector2[x.Length];
		for(int i=0; i<x.Length; i++){
			vectorized[i].x = x[i];
			vectorized[i].y = y[i];
		}
		return vectorized;
	}
	
	public static Vector3 GridSpacePosition(Vector2[] shape, Vector2 position, float scale=1)
    {
		Vector2 boxCenter = BoundingBoxCenterOfCoordenates( shape );
		Vector2 gridSpacePosition = (boxCenter + position) * scale;
		Vector3 gridSpacePosition3D = new Vector3( gridSpacePosition.x, 0, gridSpacePosition.y );

		return gridSpacePosition3D;
	}


}

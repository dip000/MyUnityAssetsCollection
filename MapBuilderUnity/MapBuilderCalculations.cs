using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapBuilderCalculations : object
{
	public static Vector2 BoundingBoxAverageOfCoordenates(Vector2[] coordenates)
	{
		Vector2 averageOfLocalCoordenates = Vector2.zero;

		for (int i = 0; i < coordenates.Length; i++)
		{

			if (coordenates[i].x > averageOfLocalCoordenates.x)
			{
				averageOfLocalCoordenates.x = coordenates[i].x;
			}
			if (coordenates[i].y > averageOfLocalCoordenates.y)
			{
				averageOfLocalCoordenates.y = coordenates[i].y;
			}
		}

		averageOfLocalCoordenates *= 0.5f;

		return averageOfLocalCoordenates;
	}

	public static Vector2[] GlobalizeCoordenates(Vector2[] localCoordenates, Vector2 positionIndex){
		Vector2[] coordenates = new Vector2[ localCoordenates.Length ];
		
		for(int i=0; i<localCoordenates.Length; i++){
			coordenates[i] = localCoordenates[i] + positionIndex;
		}
		return coordenates;
	}

	public static Vector2[] RotateMatrixAngle(Vector2[] vector, int angle)
	{
		//Only by 90 degrees and max 3 times
		if (angle < 0)
		{
			angle = 4 - (angle % 360) / 90;
		}
		else
		{
			angle = -(angle % 360) / 90;
		}

		if (angle == 4)
			return vector;

		return RotateMatrixTimes(vector, angle);
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

}

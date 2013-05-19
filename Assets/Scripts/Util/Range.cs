using UnityEngine;
//using System;

/// <summary>
/// Couple of float numbers defining a minimum and a maximum.
/// </summary>
[System.Serializable] // To make it editable in Unity
public class Range
{	
	public float min;
	public float max;
	
	public Range(float pmin, float pmax)
	{
		min = pmin;
		max = pmax;
	}
	
	/// <summary>
	/// Gets an interpolated value from the range.
	/// </summary>
	/// <param name='t'>
	/// Position in [0, 1]
	/// </param>
	public float Lerp(float t)
	{
		return Mathf.Lerp(min, max, t);
	}
	
	/// <summary>
	/// Gets a number in [0, 1] from a value the scale of the range.
	/// </summary>
	/// <returns>
	/// Value in [0, 1]
	/// </returns>
	/// <param name='x'>
	/// Value in the range [min, max]
	/// </param>
	public float GetT(float x)
	{
		if(max != min)
			return (x - min) / (max - min);
		return 1;
	}
	
	/// <summary>
	/// Tests if x is contained in the interval.
	/// Edges are included.
	/// </summary>
	public bool Contains(float x)
	{
		return x >= min && x <= max;
	}
	
	/// <summary>
	/// Gets the middle value in the range from linear interpolation.
	/// </summary>
	public float Mid
	{
		get { return 0.5f * (min + max); }
	}
	
	/// <summary>
	/// Returns a value of x clamped to the interval.
	/// </summary>
	public float Clamp(float x)
	{
		if(x > max)
			return max;
		if(x < min)
			return min;
		return x;
	}
	
	public float Rand ()
	{
		return Random.Range(min, max);
	}

}


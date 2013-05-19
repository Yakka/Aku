using UnityEngine;
using System;

/// <summary>
/// When Mathf is not enough.
/// </summary>
public class Helper
{
	/// <summary>
	/// Returns the sign of x, and 0 if is 0.
	/// </summary>
	/// <param name='x'>
	/// 1 if x > 0,
	/// -1 if x < 0,
	/// 0 if x == 0.
	/// </param>
	public static int Sign0(int x)
	{
		return x>0? 1 : x<0? -1 : 0;
	}
	
	public static float Sq(float x)
	{
		return x*x;
	}
	
	public static float Match(float a, float b, float tolerance)
	{
		if(tolerance == 0)
			return a == b ? 1 : 0;
		return Mathf.Clamp(1f - Mathf.Abs(a - b) / tolerance, 0, 1);
	}
	
	public static float ColorMatch(Color a, Color b, float tolerance)
	{
		return 0.333f * (
			Match(a.r, b.r, tolerance) + 
			Match(a.g, b.g, tolerance) + 
			Match(a.b, b.b, tolerance));
	}
	
	public static float SqrDistance(float x1, float y1, float x2, float y2)
	{
		return Sq(x2-x1) + Sq(y2-y1);
	}
	
	public static void SetQuadMeshColors(GameObject obj, Color color)
	{
		MeshFilter mf = obj.GetComponent<MeshFilter>();
		Color32[] colors = mf.mesh.colors32;
		if(colors.Length == 0)
			colors = new Color32[4];
		for(int i = 0; i < colors.Length; ++i)
		{
			colors[i] = color;
		}
		mf.mesh.colors32 = colors;
	}

}


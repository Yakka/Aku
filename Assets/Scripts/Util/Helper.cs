using UnityEngine;
using System;

/// <summary>
/// When Mathf is not enough.
/// </summary>
public class Helper
{
	/// <summary>
	/// Returns the sign of x (-1 or 1), and 0 if x == 0.
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
	
	/// <summary>
	/// Returns the sign of x (-1 or 1), and 0 if x == 0.
	/// </summary>
	/// <param name='x'>
	/// 1 if x > 0,
	/// -1 if x < 0,
	/// 0 if x == 0.
	/// </param>
	public static float Sign0(float x)
	{
		if(x > 0)
			return 1;
		if(x < 0)
			return -1;
		return 0;
	}
	
	public static float AngleRad(float x, float y)
	{
		float d = Mathf.Sqrt(x*x + y*y);
		return Mathf.Atan2(y/d, x/d);
	}
	
	public static float DeltaAngleRad(float current, float target)
	{
		float num = Mathf.Repeat (target - current, 2f*Mathf.PI);
		if (num > Mathf.PI)
		{
			num -= 2f*Mathf.PI;
		}
		return num;
	}
	
	public static float Sq(float x)
	{
		return x*x;
	}
	
	public static float Pow4(float x)
	{
		return x*x*x*x;
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
	
	public static GameObject Random(GameObject[] arr)
	{
		return arr[UnityEngine.Random.Range(0, arr.Length-1)];
	}
	
	/// <summary>
	/// Centralized function to recursively set the active flag of an object
	/// (Because depending on the version of Unity, activate an object is done a different way).
	/// </summary>
	/// <param name='obj'></param>
	/// <param name='a'></param>
	public static void SetActive(GameObject obj, bool a)
	{
		obj.active = a; // Use this on Unity 3.5.6-
		//obj.SetActive(a);
		//obj.activeSelf = a; // Use this on Unity 3.5.7+
		
		for(int i = 0; i < obj.transform.childCount; ++i)
			SetActive(obj.transform.GetChild(i).gameObject, a);
	}
	
	public static bool IsActive(GameObject obj)
	{
		return obj.active;
		//return obj.activeSelf;
	}

}


using UnityEngine;
using System.Collections;

/// <summary>
/// Global information about halos (not Unity's halos).
/// </summary>
public class HaloManager : MonoBehaviour // YETANOTHERMANAGER
{
	public AnimationCurve startCurve; // Start size animation
	public AnimationCurve idleCurve; // Periodic size animation
	public AnimationCurve endCurve; // End size animation
	//public AnimationCurve blinkCurve;
	
	private static HaloManager globalInstance;
	//private GameObject[] blinkPool;
	
	public static HaloManager Instance
	{
		get { return globalInstance; }
	}
	
	void Awake()
	{
		globalInstance = this;
	}
	
	void Start()
	{
		// If curves are not defined from editor, 
		// default curves (not as fancy) are generated.
		if(startCurve == null)
		{
			startCurve = new AnimationCurve();
			startCurve.AddKey(0f, 0f);
			startCurve.AddKey(0.5f, 1.2f);
			startCurve.AddKey(1f, 1f);
		}
		if(idleCurve == null)
		{
			idleCurve = new AnimationCurve();
			idleCurve.AddKey(0f, 1f);
			idleCurve.AddKey(0.25f, 1.1f);
			idleCurve.AddKey(0.75f, 0.9f);
			idleCurve.AddKey(1f, 1f);
		}
		if(endCurve == null)
		{
			endCurve = new AnimationCurve();
			endCurve.AddKey(0f, 1f);
			endCurve.AddKey(0f, 0f);
		}
//		if(blinkCurve == null)
//		{
//			blinkCurve = new AnimationCurve();
//			blinkCurve.AddKey(0f, 0f);
//			blinkCurve.AddKey(0.5f, 1f);
//			blinkCurve.AddKey(1f, 0f);
//		}
		
//		blinkPool = new GameObject[4];
//		for(int i = 0; i < blinkPool.Length; ++i)
//		{
//		}
	}
	
//	void SpawnBlink(float x, float y)
//	{
//		
//	}
	
}


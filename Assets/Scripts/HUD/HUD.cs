using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RingWaveManager))]
public class HUD : MonoBehaviour 
{
	private static HUD globalInstance;
	
	//public Transform handPrefab;
	
	private RingWaveManager ringWaveManager;
	private GameObject indicatorSketchObj;
	private IndicatorSketch indicatorSketch;	
	private TouchController touchControllerRef;
	private float assistTouchTime;

	void Awake() { globalInstance = this; }
	public static HUD Instance { get { return globalInstance; } }

	void Start ()
	{
		ringWaveManager = GetComponent<RingWaveManager>();
		
		indicatorSketchObj = new GameObject();
		indicatorSketchObj.AddComponent<IndicatorSketch>();
		indicatorSketchObj.transform.parent = this.transform;
		Helper.SetActive(indicatorSketchObj, false);		
		indicatorSketch = indicatorSketchObj.GetComponent<IndicatorSketch>();
		
		GameObject tco = GameObject.Find("TouchController");
		if(tco != null)
		{
			touchControllerRef = tco.GetComponent<TouchController>();
		}
		if(touchControllerRef == null)
		{
			Debug.LogError(name + ": Couldn't locate TouchController !");
		}
	}
	
	void Update ()
	{
		if(touchControllerRef != null)
		{
			// If the gamer didn't touched anything for the first time or missed something
			if(!touchControllerRef.EverPressed 
				&& Time.time - touchControllerRef.EndTime > 10f
				&& Time.time - assistTouchTime > 7f)
			{
				// Indicate that he can !
				ringWaveManager.SpawnSeries(0,-30, 2); // Two circle waves
				indicatorSketch.Spawn(
					new Vector3(0,-30,0), 
					new Vector3(90,30,0), -12); // A curve from left to up-right
				assistTouchTime = Time.time;
			}
		}
		
		if(!Settings.onTablet)
		{
			if(Input.GetKeyDown(KeyCode.W))
			{
				ringWaveManager.SpawnSeries(0,0,2);
			}
			if(Input.GetKeyDown(KeyCode.I))
			{
				indicatorSketch.Spawn(
					new Vector3(0,-30,0), 
					new Vector3(90,30,0), -12);
			}
		}
	}

}










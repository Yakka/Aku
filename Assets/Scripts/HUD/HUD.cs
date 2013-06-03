using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RingWaveManager))]
public class HUD : MonoBehaviour 
{
	private static HUD globalInstance;
	
	//public Transform handPrefab;
	
	private RingWaveManager ringWaveManager;
	private GameObject indicatorSketch;

	void Awake() { globalInstance = this; }
	public static HUD Instance { get { return globalInstance; } }

	void Start ()
	{
		ringWaveManager = GetComponent<RingWaveManager>();
		
		GameObject obj = new GameObject();
		obj.AddComponent<IndicatorSketch>();
		obj.transform.parent = this.transform;
		indicatorSketch = obj;
		Helper.SetActive(indicatorSketch, false);
	}
	
	void Update ()
	{
		if(Input.GetKeyDown(KeyCode.W))
		{
			ringWaveManager.SpawnSeries(0,0,2);
		}
		if(Input.GetKeyDown(KeyCode.I))
		{
			indicatorSketch.GetComponent<IndicatorSketch>().Spawn(
				new Vector3(0,-30,0), new Vector3(90,30,0), -12);
		}
	}

}










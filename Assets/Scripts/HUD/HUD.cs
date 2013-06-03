using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RingWaveManager))]
public class HUD : MonoBehaviour 
{
	private static HUD globalInstance;
	
	//public Transform handPrefab;
	
	private RingWaveManager ringWaveManager;

	void Awake() { globalInstance = this; }
	public static HUD Instance { get { return globalInstance; } }
	
	void Start ()
	{
		/*Vector3 pos = transform.localPosition;
		pos.z += 1f;
		transform.localPosition = pos;*/
		
		ringWaveManager = GetComponent<RingWaveManager>();
	}
	
	void Update ()
	{
		if(Input.GetKeyDown(KeyCode.W))
			ringWaveManager.SpawnSeries(0,0,2);
	}

}

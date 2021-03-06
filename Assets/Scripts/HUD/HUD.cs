using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RingWaveManager))]
public class HUD : MonoBehaviour 
{
	public Texture2D hand;
	public int handX;
	public int handY;
	public int targetX;
	public int targetY;
	private float timerHand = 0;
	private Rect rectHand;
	
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
		assistTouchTime = Time.timeSinceLevelLoad;
		
		ringWaveManager = GetComponent<RingWaveManager>();
		
		indicatorSketchObj = new GameObject();
		indicatorSketchObj.AddComponent<IndicatorSketch>();
		indicatorSketchObj.transform.parent = this.transform;
		Helper.SetActive(indicatorSketchObj, false);		
		indicatorSketch = indicatorSketchObj.GetComponent<IndicatorSketch>();
		rectHand = new Rect(handX, handY, 128, 128);
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
			float time = Time.timeSinceLevelLoad;
			
			// If the gamer didn't touched anything for the first time or missed something
			if(!touchControllerRef.EverPressed 
				&& time - touchControllerRef.EndTime > 5f
				&& time - assistTouchTime > 5f)
			{
				// Indicate that he can !
				ringWaveManager.SpawnSeries(0,-30, 2); // Two circle waves
				indicatorSketch.Spawn(
					new Vector3(0,-30,0), 
					new Vector3(90,30,0), -12); // A curve from left to up-right
				assistTouchTime = time;
				timerHand = time + 2f;
				rectHand.x = handX;
				rectHand.y = handY;
			}
			if(time < timerHand)
			{
				rectHand.x = Mathf.Lerp(rectHand.x, targetX, 0.05f);
				rectHand.y = Mathf.Lerp(rectHand.y, targetY, 0.05f);
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
	
	void OnGUI()
	{
		Texture tex = Camera.mainCamera.GetComponent<CameraHandler>().renderTarget;
		if(tex != null)
		{
			// FIXME Uses Y-inverted screen coordinates and so breaks the reveal shader,
			// causing every hidden painting to be inverted Y-wise -_-"
			int w = Screen.width;
			int h = Screen.height;
			if(w < 10 || h < 10)
				Debug.LogError(name + ": BAD SCREEN " + w + ", " + h);
			if(tex.width < 10 || tex.height < 10)
				Debug.LogError(name + ": BAD TEX " + w + ", " + h);
			GUI.DrawTexture(new Rect(0,0, w, h), tex);
		}
    }

}










using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Global manager for ponctual polygon-based paintings.
/// </summary>
public class PolyPaintManager : MonoBehaviour 
{
	private static PolyPaintManager globalInstance;
	
	public GameObject[] projectionPrefab;
	public GameObject[] dripPrefab;
	public GameObject[] splashPrefab;
	public Material pixelPaintMaterial;
	public AnimationCurve decreaseCurve;
	private List<PolyPaint> animatedPaints = new List<PolyPaint>();
	// TODO 3 indexed colors here for the second level
	
	private float z;
	
	void Awake()
	{
		globalInstance = this;
		z = transform.position.z;
	}
	
	public static PolyPaintManager Instance
	{
		get { return globalInstance; }
	}
	
	public void SpawnCloudProjection(Color color, float x, float y, float angleDeg)
	{
		Vector3 pos = new Vector3(x, y, z);
		GameObject obj = Instantiate(
			projectionPrefab[Random.Range(0, projectionPrefab.Length-1)], 
			pos, Quaternion.Euler(0,0,angleDeg)) as GameObject;
		
		Level.Get.Attach(obj); // For wrapping
		
		obj.name = "PolyPaint_projection";
		
		// Fix position
		float r = 8f;
		pos.x -= r*Mathf.Cos(angleDeg * Mathf.Deg2Rad);
		pos.y -= r*Mathf.Sin(angleDeg * Mathf.Deg2Rad);
		obj.transform.position = pos;
		
		PolyPaint pp = new PolyPaint(obj, color, 0.25f);
		animatedPaints.Add(pp);
	}
	
	public void SpawnDrip(Color color, float x, float y)
	{
		Vector3 pos = new Vector3(x, y, z);
		GameObject obj = Instantiate(
			dripPrefab[Random.Range(0, dripPrefab.Length-1)], 
			pos, Quaternion.Euler(0,0,Random.Range(-180, 180))) as GameObject;
		
		Level.Get.Attach(obj); // For wrapping

		float s = obj.transform.localScale.x * Random.Range(0.8f, 1.3f);
		obj.transform.localScale = new Vector3(s,s,1);
		
		obj.name = "PolyPaint_drip";
		
		// This one is static		
		// Send color by mesh vertices			
		MeshFilter mf = obj.GetComponent<MeshFilter>();
		//Color32[] colors = mf.mesh.colors32;
		Color32[] colors = new Color32[4];//mf.mesh.colors32;
		for(int i = 0; i < colors.Length; ++i)
		{
			colors[i] = color;
		}
		mf.mesh.colors32 = colors;
	}
	
	public void SpawnSplash(Color color, float x, float y, float scaleMultiplier)
	{
		Vector3 pos = new Vector3(x, y, z);
		GameObject obj = Instantiate(
			splashPrefab[Random.Range(0, splashPrefab.Length-1)], 
			pos, Quaternion.Euler(0,0,Random.Range(-180, 180))) as GameObject;
		
		Level.Get.Attach(obj); // For wrapping

		float s = obj.transform.localScale.x * Random.Range(0.8f, 1.3f) * scaleMultiplier;
		obj.transform.localScale = new Vector3(s,s,1);
		
		obj.name = "PolyPaint_splash";
		
		PolyPaint pp = new PolyPaint(obj, color, 0.25f);
		animatedPaints.Add(pp);
		
		// TODO Use colorIndexes
		// Check revealed parts on hidden paintings
		// Note : s is reduced a bit because the splash doesn't spread much paint on its edges
		CheckHiddenPaintings(pos, 0.8f*s, 0);
	}
	
	private void CheckHiddenPaintings(Vector3 pos, float radius, int colorIndex)
	{
		Collider[] colliders = Physics.OverlapSphere(pos, radius);
		for(int i = 0; i < colliders.Length; ++i)
		{
			HiddenPainting hp = colliders[i].GetComponent<HiddenPainting>();
			if(hp != null)
			{
				hp.CheckRevealZone(pos.x, pos.y, radius, colorIndex);
			}
		}
	}
	
	void Update()
	{
		foreach(PolyPaint pp in animatedPaints)
		{
			pp.Update();
		}
	}

}



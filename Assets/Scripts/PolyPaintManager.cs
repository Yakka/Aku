using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Global manager for polygon-based paintings.
/// Also provides ponctual paint spawn methods.
/// </summary>
public class PolyPaintManager : MonoBehaviour 
{
	private static PolyPaintManager globalInstance;
	
	public GameObject[] projectionPrefab;
	public GameObject[] dripPrefab;
	public GameObject[] splashPrefab;
	public Material monoPolyPaintMaterial;
	public Material triPolyPaintMaterial;
	public Material monoPixelPaintMaterial;
	public Material triPixelPaintMaterial; // Reveal material used in level 2
	public AnimationCurve decreaseCurve; // Paint intensity from its charge
	private List<PolyPaintQuad> animatedPaints = new List<PolyPaintQuad>();
	public Color[] indexedColors; // All paint colors used in the level
	
	private float z;
	public float offZ = 0;
	
	void Awake()
	{
		globalInstance = this;
		z = transform.position.z;
	}
	
	void Start()
	{
		CheckStuff();
		
		// Was useful before because the reveal shader was applied on the prefabs.
		// Now, revealation is done on a quad the size of the screen.
		/*
		if(Level.Get.levelID == 2)
		{
			// Switch materials of prefabs because the level 2
			// uses a different paint reveal shader
			string shaderName = "Custom/TriPaintReveal";
			Shader shader = Shader.Find(shaderName);
			if(shader != null)
			{
				Material commonMat = new Material(shader);
				// Red, gold yellow, blue (Level2-specific)
				commonMat.SetColor("_Color1", new Color(1f, 0f, 0f));
				commonMat.SetColor("_Color2", new Color(1f, 0.75f, 0f));
				commonMat.SetColor("_Color3", new Color(0f, 0f, 1f));
				
				SwitchPrefabsMaterial(dripPrefab, commonMat);
				SwitchPrefabsMaterial(splashPrefab, commonMat);
				SwitchPrefabsMaterial(projectionPrefab, commonMat);
			}
			else
			{
				Debug.LogError(name + ": " + shaderName + " shader not found !");
			}
		}
		*/
	}
	
	public float GetNextZ()
	{
		offZ += 0.00001f;
		//offZ += 0.01f;
		return z-offZ;
	}

	/*private static void SwitchPrefabsMaterial(GameObject[] prefabs, Material mat)
	{
		for(int i = 0; i < prefabs.Length; ++i)
		{
			// I make a copy from each prefab because otherwise they will be
			// modified into the project, causing other scenes to break
			GameObject obj = Instantiate(prefabs[i]) as GameObject;
			obj.active = false;
			prefabs[i] = obj;
			
			Renderer r = prefabs[i].renderer;
			Material newMat = new Material(mat);
			Texture texture = r.material.mainTexture;
//			if(texture == null)
//				Debug.Log("FU");
			newMat.SetTexture("_MainTex", texture);
			r.material = newMat;
		}
	}*/
	
	public static PolyPaintManager Instance
	{
		get { return globalInstance; }
	}
	
	public Material PixelPaintMaterial
	{
		get
		{
			if(Level.Get.levelID == 2)
				return triPixelPaintMaterial;
			else
				return monoPixelPaintMaterial;
		}
	}
	
	public Material PolyPaintMaterial
	{
		get
		{
			if(Level.Get.levelID == 2)
				return triPolyPaintMaterial;
			else
				return monoPolyPaintMaterial;
		}
	}

	public Color ColorFromIndex(int index)
	{
		if(Level.Get.levelID == 2)
		{
			switch(index)
			{
			case 0 : return Color.red;
			case 1 : return Color.green;
			case 2 : return Color.blue;
			default : return Color.white;
			}
		}
		else
		{
			if(index >= 0 && index < indexedColors.Length)
				return indexedColors[index];
			else
				return Color.white;
		}
	}
	
	public void SpawnCloudProjection(int index, float x, float y, float angleDeg)
	{
		SpawnCloudProjection(ColorFromIndex(index), x, y, angleDeg);
	}
	
	public void SpawnCloudProjection(Color color, float x, float y, float angleDeg)
	{
		Vector3 pos = new Vector3(x, y, GetNextZ());
		GameObject obj = Instantiate(
			Helper.Random(projectionPrefab),
			pos, Quaternion.Euler(0,0,angleDeg)) as GameObject;
		
		Level.Get.Attach(obj); // For wrapping
		
		obj.name = "PolyPaint_projection";
		obj.active = true; // Needed when prefabs are inactive modified clones instead
		
		// Fix position
		float r = 8f;
		pos.x -= r*Mathf.Cos(angleDeg * Mathf.Deg2Rad);
		pos.y -= r*Mathf.Sin(angleDeg * Mathf.Deg2Rad);
		obj.transform.position = pos;
		
		PolyPaintQuad pp = new PolyPaintQuad(obj, color, 0.25f);
		animatedPaints.Add(pp);
		
		// Cloud projection don't check revelations (maybe it should?)
	}
	
	public void SpawnDrip(int index, float x, float y)
	{
		SpawnDrip(ColorFromIndex(index), x, y);
	}
	
	public void SpawnDrip(Color color, float x, float y)
	{
		Vector3 pos = new Vector3(x, y, GetNextZ());
		GameObject obj = Instantiate(
			Helper.Random(dripPrefab),
			pos, Quaternion.Euler(0,0,Random.Range(-180, 180))) as GameObject;
		
		Level.Get.Attach(obj); // For wrapping

		float s = obj.transform.localScale.x * Random.Range(0.8f, 1.3f);
		obj.transform.localScale = new Vector3(s,s,1);
		
		obj.name = "PolyPaint_drip";
		obj.active = true; // Needed when prefabs are inactive modified clones instead
		
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
		
		// Drips don't check revelations (trails are more relevant)
	}
	
	public void SpawnSplash(int colorIndex, float x, float y, float scaleMultiplier)
	{
		SpawnSplash(colorIndex, ColorFromIndex(colorIndex), x, y, scaleMultiplier);
	}
	
	public void SpawnSplash(int colorIndex, Color color, float x, float y, float scaleMultiplier)
	{
		if(Level.Get.levelID == 2)
		{
			color = ColorFromIndex(colorIndex);
		}
		
		Vector3 pos = new Vector3(x, y, GetNextZ());
		GameObject obj = Instantiate(
			Helper.Random(splashPrefab),
			pos, Quaternion.Euler(0,0,Random.Range(-180, 180))) as GameObject;
		
		Level.Get.Attach(obj); // For wrapping

		float scaling = obj.transform.localScale.x * Random.Range(0.8f, 1.3f) * scaleMultiplier;
		obj.transform.localScale = new Vector3(scaling, scaling, 1);
		
		obj.name = "PolyPaint_splash";
		obj.active = true; // Needed when prefabs are inactive modified clones instead
		
		PolyPaintQuad pp = new PolyPaintQuad(obj, color, 0.25f);
		animatedPaints.Add(pp);
		
		// Check revealed parts on hidden paintings
		// Note : scaling is reduced a bit because the splash doesn't spread much paint on its edges
		CheckHiddenPaintings(pos, 0.8f*scaling, colorIndex);
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
		foreach(PolyPaintQuad pp in animatedPaints)
		{
			pp.Update();
		}
	}
	
	private void CheckStuff()
	{
		if(projectionPrefab.Length == 0 || projectionPrefab[0] == null)
			Debug.LogError(name + ": projection prefab(s) are not defined !");
		if(dripPrefab.Length == 0 || dripPrefab[0] == null)
			Debug.LogError(name + ": drip prefab(s) are not defined !");
		if(splashPrefab.Length == 0 || splashPrefab[0] == null)
			Debug.LogError(name + ": splash prefab(s) are not defined !");

		if(monoPolyPaintMaterial == null)
			Debug.LogError(name + ": polyPaintMaterial is not defined !");
		if(triPolyPaintMaterial == null)
			Debug.LogError(name + ": triPolyPaintMaterial is not defined !");
		if(monoPixelPaintMaterial == null)
			Debug.LogError(name + ": pixelPaintMaterial is not defined !");
		if(triPixelPaintMaterial == null)
			Debug.LogError(name + ": triPixelPaintMaterial is not defined !");
	}

}



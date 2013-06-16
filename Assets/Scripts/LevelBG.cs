using UnityEngine;
using System.Collections;

public class LevelBG : MonoBehaviour
{
	public Material glowMaterial;
	public AnimationCurve mixCurve;
	//private Mesh mesh;
	//private Color[] vcolors;
	
	private static LevelBG globalInstance;
	
	void Awake()
	{
		globalInstance = this;
	}
	
	public static LevelBG Instance
	{
		get { return globalInstance; }
	}
	
	void Start ()
	{
		/*
		MeshFilter mf = GetComponent<MeshFilter>();
		mesh = mf.mesh;
		vcolors = new Color[mesh.vertices.Length];
		
		Level level = Level.Get;
		transform.localScale = new Vector3(level.Width, level.Height, 1);
		
		Material mat = GetComponent<MeshRenderer>().material;
		Vector2 s = new Vector2(level.widthTiles, level.heightTiles);
		mat.SetTextureScale("_MainTex", s);
		mat.SetTextureScale("_Texture2", s);
		*/
		
		if(glowMaterial == null)
			Debug.LogError("glowMaterial not defined in LevelBG !");
	}
	
	void Update ()
	{
		Level level = Level.Get;
		
		//
		// Update wrapping
		//
		
		
		//float camX = Camera.mainCamera.transform.position.x;
		float camY = Camera.mainCamera.transform.position.y;
		/*
		int tx, ty;
		level.WorldToTileCoords(camX, camY, out tx, out ty, false);
		Vector3 pos = transform.position;
		pos.x = (tx - level.centerXTiles) * Tile.SIZE;
		pos.y = (ty - level.centerYTiles) * Tile.SIZE;
		transform.position = pos;
		*/
		
		//
		// Update texture mixing
		//
		
		float y = level.GetWrappedAltitude(camY);
		float h = y / level.Height;
		float t = Mathf.Clamp(mixCurve.Evaluate(h), 0f, 1f);
		//float t = (0.5f * Mathf.Cos(Time.time) + 0.5f);
		
		/*
		for(int i = 0; i < vcolors.Length; ++i)
		{
			vcolors[i].a = t;
		}
		
		mesh.colors = vcolors;
		*/
		
		//
		// Update glows
		//
		
		if(glowMaterial != null)
		{
			float k = 0.6f * (1f - t);
			glowMaterial.SetColor("_TintColor", new Color(k,k,k,1f));
		}
		
		//Debug.Log(mixCurve.Evaluate(0.01f));
	}
	
}






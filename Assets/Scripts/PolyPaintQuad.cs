using UnityEngine;
using System.Collections;

/// <summary>
/// This controls the lifetime of a ponctual painting using polygons.
/// </summary>
public class PolyPaintQuad 
{
	private const float INITIAL_SPREAD = 0.8f;
	
	/// <summary>
	/// Spread effect factor : inverted value ! 
	/// 1 means minimum spread, 0 means maximum spread.
	/// </summary>
	public float spread = INITIAL_SPREAD;
	public float spreadTime = 3f;
	
	/// <summary>
	/// If set to true, no shader properties will be changed on update,
	/// in order to allow batching and increase performance.
	/// However, it will disable shader-based animations.
	/// </summary>
	public bool dontBreakBatching = true;

	private GameObject gameObject;
	private float startTime;
	private float finalScale;
	
	private bool requestedImpress;
	
	public PolyPaintQuad(GameObject obj, Color color, float spreadTime)
	{		
		gameObject = obj;
		startTime = Time.time;
		finalScale = obj.transform.localScale.x;
		this.spreadTime = spreadTime;
		
		if(dontBreakBatching)
		{
			if(spreadTime > 0.001f)
			{
				//Debug.Log("aaaaaaa");
				gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 1f);
			}
			
			// Send color by mesh vertices			
			MeshFilter mf = gameObject.GetComponent<MeshFilter>();
			Color32[] colors = new Color32[4];//mf.mesh.colors32;
			//Debug.Log(colors.Length);
			for(int i = 0; i < colors.Length; ++i)
			{
				colors[i] = color;
			}
			mf.mesh.colors32 = colors;
		}
		else
		{
			// Send color directly in shader parameter
			gameObject.renderer.material.SetColor("_Color", color);
		}
	}
	
	public void Update ()
	{
		if(gameObject == null)
			return;
		
		float span = Time.time - startTime;
		if(span < spreadTime)
		{
			spread = Helper.Sq(1f - span / spreadTime);
			if(dontBreakBatching)
			{
				// Use the scale to simulate spreading
				gameObject.transform.localScale = 
					new Vector3(
						finalScale * (1f-spread), 
						finalScale * (1f-spread), 1f);
			}
			else
			{
				// Use contrast and thresholds to simulate spreading
				gameObject.renderer.material.SetFloat("_Spread", spread);
			}
		}
		else if(!requestedImpress)
		{
			RequestImpress();
		}
		else
		{
			Level.Get.Detach(gameObject);
			// TODO Fix that, seems like Detach has no effect and MissingReferences occur
			//GameObject.Destroy(gameObject); 
			Helper.SetActive(gameObject, false); 
			gameObject = null;
		}
	}
	
	private void RequestImpress()
	{
		// Change layer
		gameObject.layer = LayerMask.NameToLayer("Impress");
		
		// Change material
		Material mat = Helper.CreateMaterial("Mobile/Particles/Alpha Blended");
		Renderer r = gameObject.renderer;
		mat.mainTexture = r.material.mainTexture;
		r.material = mat;
		
		// TODO optimize splash impress
		Vector3 pos = gameObject.transform.position;
		int x, y;
		Level.Get.WorldToTileCoords(pos.x, pos.y, out x, out y, false);
		Level.Get.GetTile( x-1,	y-1 ).RequestPaintImpress(gameObject);
		Level.Get.GetTile( x,	y-1 ).RequestPaintImpress(gameObject);
		Level.Get.GetTile( x+1,	y-1 ).RequestPaintImpress(gameObject);
		Level.Get.GetTile( x-1,	y   ).RequestPaintImpress(gameObject);
		Level.Get.GetTile( x,	y   ).RequestPaintImpress(gameObject);
		Level.Get.GetTile( x+1,	y   ).RequestPaintImpress(gameObject);
		Level.Get.GetTile( x-1,	y+1 ).RequestPaintImpress(gameObject);
		Level.Get.GetTile( x,	y+1 ).RequestPaintImpress(gameObject);
		Level.Get.GetTile( x+1,	y+1 ).RequestPaintImpress(gameObject);
		requestedImpress = true;
	}

}








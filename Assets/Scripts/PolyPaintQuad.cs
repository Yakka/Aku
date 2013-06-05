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
	}
	
}





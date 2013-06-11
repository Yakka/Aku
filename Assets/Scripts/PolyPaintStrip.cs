using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PolyPaintStrip : MonoBehaviour
{
	const int MAX_POINTS = 256;
	const int IMPRESS_QUEUE_MAX_LENGTH = 16;
	private static int instanceCount;
	private static Material commonRevealMaterial;
	private static List<PolyPaintStrip> impressQueue = new List<PolyPaintStrip>();
	
	private PolyPaintStrip next;
	
	private Mesh mesh;
	private Vector3[] points;
	private int pointIndex;
	private int fadeIndex = -1;
	private bool requestedImpress = false;
	private bool canImpress = true;
	private bool hasBeenQueued = false;
	private bool finished;
	
	public Vector3[] vertices = new Vector3[0];
	private int[] triangles = new int[0]; // Triangle indices
	private Vector2[] uv = new Vector2[0];
	private Color32[] colors = new Color32[0];
	
	void Start ()
	{
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		mesh.name = "generated_paint_strip";
		
//		if(commonRevealMaterial == null)
//		{
//			commonMaterial = Helper.CreateMaterial("VertexColor");
//		}
		gameObject.renderer.material = PolyPaintManager.Instance.PolyPaintMaterial;
		
		Level.Get.Attach(gameObject);
		
		gameObject.layer = LayerMask.NameToLayer("PaintReveal");
		
		++instanceCount;
		
//		points = new Vector3[MAX_POINTS];
//		for(int i = 0; i < points.Length; ++i)
//			points[i] = new Vector3();
	}
	
	public bool CanImpress
	{
		get { return canImpress; }
		set { canImpress = value; }
	}
	
	//private static int strippingCount = 0;
	public bool Plot(Vector3 pos, float angleRad, float thickness, Color color)
	{
		if(finished || pointIndex == MAX_POINTS)
			return false;
		if(points == null)
		{
			//++strippingCount;
			//Debug.Log("+ " + strippingCount);
			points = new Vector3[MAX_POINTS];
			for(int i = 0; i < points.Length; ++i)
				points[i] = new Vector3();
		}
		points[pointIndex] = pos;
		
		pos -= transform.position;
		pos.z = PolyPaintManager.Instance.GetNextZ();
		
		int vi = vertices.Length;
		
		System.Array.Resize<Vector3>(ref vertices, vertices.Length+2);
		System.Array.Resize<Vector2>(ref uv, uv.Length+2);
		System.Array.Resize<Color32>(ref colors, colors.Length+2);
		
		float ux = Mathf.Cos(angleRad + Mathf.PI/2f);
		float uy = Mathf.Sin(angleRad + Mathf.PI/2f);
		
		vertices[vi] = new Vector3(pos.x-ux*thickness, pos.y-uy*thickness, pos.z);
		vertices[vi+1] = new Vector3(pos.x+ux*thickness, pos.y+uy*thickness, pos.z);
		
		if(pointIndex % 2 == 0)
		{
			uv[vi] = new Vector2(0,0);
			uv[vi+1] = new Vector2(0,1);
		}
		else
		{
			uv[vi] = new Vector2(1,0);
			uv[vi+1] = new Vector2(1,1);
		}
		
		colors[vi] = color;
		colors[vi+1] = color;
		
		if(vertices.Length >= 4)
		{			
			int ti = triangles.Length;
			
			System.Array.Resize<int>(ref triangles, triangles.Length+6);
			int lti = ti != 0 ? triangles[ti-2] : 0;
			
			triangles[ti] = lti;
			triangles[ti+1] = lti+1;
			triangles[ti+2] = lti+2;
			triangles[ti+3] = lti+3;
			triangles[ti+4] = lti+2;
			triangles[ti+5] = lti+1;
			
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.uv = uv;
			mesh.colors32 = colors;
			
			//Debug.Log("StripClose " + pointIndex);
		}
		
		++pointIndex;
		if(pointIndex == MAX_POINTS)
		{
			Finish();
			return false;
		}
		else
			return true;
	}
	
	public bool Finished
	{
		get { return finished; }
	}
	
	public void Finish()
	{
		if(!finished)
		{
			//Debug.Log("Strip finished");
			finished = true;
			//--strippingCount;
			//Debug.Log("- " + strippingCount);
			//Fade();
		}
	}
	
	public void Fade()
	{
		if(requestedImpress && !hasBeenQueued)
			Debug.Log("CACAAAA");
		if(fadeIndex == -1)
			fadeIndex = 0;
	}
	
	void Update()
	{
		// If fading is active
		if(fadeIndex >= 0 && fadeIndex < colors.Length)
		{
			// If there is at least 2 triangles, apply fading.
			if(mesh.vertices.Length >= 4)
			{
				int d = 0, i = 0;
				for(int a = 0; a < 255; a += 32)
				{
					i = fadeIndex + d;
					if(i < colors.Length)
					{
						colors[i].a = (byte)a;
					}
					++d;
				}
				
				mesh.colors32 = colors;
			}
			
			++fadeIndex;
			
			// If fading reached the end of the strip
			if(fadeIndex == colors.Length)
			{
				if(next != null)
				{
					// Start fading the next strip
					PolyPaintStrip next_script = next.GetComponent<PolyPaintStrip>();
					if(!next_script.CanImpress)
						next_script.Fade(); // Only not impressible nexts are automatically faded
				}
				Destroy();
			}
		}
		else if(Finished && CanImpress)
		{
			// TODO introduce an interval in which several strips attend for being printed together,
			// because drake scales finish their strips one by one on each frame,
			// causing too much useless renderings (they could be done in one).
			if(!hasBeenQueued && !requestedImpress)
			{
				impressQueue.Add(this);
				hasBeenQueued = true;
					
				if(impressQueue.Count >= IMPRESS_QUEUE_MAX_LENGTH)
				{
					foreach(PolyPaintStrip pps in impressQueue)
					{
						pps.RequestImpress();
						pps.Fade();
					}
					impressQueue.Clear();
					//Debug.Log("Dequeued " + impressQueue.Count);
				}
			}
		}
	}
	
	private void RequestImpress()
	{
		gameObject.layer = LayerMask.NameToLayer("Impress");
		Level.Get.RequestPaintImpress(gameObject);
		Material mat = gameObject.renderer.material;
		mat.shader = Helper.FindShader("Mobile/Particles/Alpha Blended");
		//gameObject.renderer.material = PolyPaintManager.commonAlphaMaterial;
		requestedImpress = true;
	}
	
	private void Destroy()
	{
		Level.Get.Detach(this.gameObject);
		Destroy(this.gameObject);
		--instanceCount;
	}
	
	public Bounds Bounds
	{
		get { return mesh.bounds; }
	}
	
	public PolyPaintStrip Next
	{
		get { return next; }
		set
		{
			if(value == this)
				Debug.LogError(name + ": cannot set myself as Next !");
			else
				next = value;
		}
	}
	
	void PreLevelWrap()
	{
		Finish();
	}
	
	void PostLevelWrap()
	{
		//Debug.Log(gameObject.name + ": PostLevelWrap");
		Finish();
	}

}


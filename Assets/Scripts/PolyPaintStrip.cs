using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PolyPaintStrip : MonoBehaviour
{
	const int MAX_POINTS = 256;
	private static int instanceCount;
	private static Material commonMaterial;
	
	private PolyPaintStrip next;
	
	private Mesh mesh;
	private Vector3[] points;
	private int pointIndex;
	private int fadeIndex = -1;
	
	private bool finished;
	
	private Vector3[] vertices = new Vector3[0];
	private int[] triangles = new int[0]; // Triangle indices
	private Vector2[] uv = new Vector2[0];
	private Color32[] colors = new Color32[0];
	
	void Start ()
	{
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		mesh.name = "generated_paint_strip";
		
		//Material material = PolyPaintManager.Instance.PolyPaintMaterial;
		if(commonMaterial == null)
			commonMaterial = Helper.CreateMaterial("Custom/VertexColor");
		GetComponent<MeshRenderer>().material = commonMaterial;
		
		Level.Get.Attach(gameObject);
		
		gameObject.layer = LayerMask.NameToLayer("PaintReveal");
		
		++instanceCount;
		
//		points = new Vector3[MAX_POINTS];
//		for(int i = 0; i < points.Length; ++i)
//			points[i] = new Vector3();
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
		pos.z -= PolyPaintManager.Instance.GetNextZ();
		
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
		if(fadeIndex == -1)
			fadeIndex = 0;
	}
	
	void Update()
	{
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
			
			if(fadeIndex == colors.Length)
			{
				if(next != null)
				{
					next.GetComponent<PolyPaintStrip>().Fade();
				}
				//Helper.SetActive(gameObject, false);
				Level.Get.Detach(this.gameObject);
				Destroy(this.gameObject);
				--instanceCount;
			}
		}
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


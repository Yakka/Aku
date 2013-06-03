using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class IndicatorSketch : MonoBehaviour 
{
	private const float THICKNESS = 2f;
	private const int NB_VERTICES = 32;
	
	private LineRenderer lineRenderer;
	private Vector3[] points;
	private int beginIndex;
	private int endIndex;
	private Material material;
	
	void Start ()
	{
		material = new Material(Shader.Find("Mobile/Particles/Alpha Blended"));
	}
	
	private void UpdateVertices()
	{
		if(lineRenderer == null)
		{
			Color color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
			lineRenderer = GetComponent<LineRenderer>();
			lineRenderer.material = material;
			lineRenderer.SetColors(color, color);
			lineRenderer.SetWidth(0.5f*THICKNESS, THICKNESS);
			lineRenderer.castShadows = false;
		}
		
		int vcount = endIndex - beginIndex + 1;
		lineRenderer.SetVertexCount(vcount);
		
		Vector3 center = transform.position;
		Vector3 pos = new Vector3(0,0,0);
		for(int i = beginIndex; i <= endIndex; ++i)
		{
			pos.x = center.x + points[i].x;
			pos.y = center.y + points[i].y;
			lineRenderer.SetPosition(i-beginIndex, pos);
		}
	}
	
	private float Curve(float t)
	{
		return 1f - Helper.Sq(2f*t - 1f);
	}
	
	public void Spawn(Vector3 begin, Vector3 end)
	{
		Spawn(begin, end, 0);
	}

	public void Spawn(Vector3 begin, Vector3 end, float curving)
	{
		beginIndex = 0;
		endIndex = 0;
		points = new Vector3[NB_VERTICES];
		
		Vector3 pos = new Vector3();
		
		Vector2 u = new Vector2(end.x-begin.x, end.y-begin.y);
		u.Normalize();
		// Rotate 90
		float tmpx = u.x;
		u.x = -u.y * curving;
		u.y = tmpx * curving;
		
		for(int i = 0; i < points.Length; ++i)
		{
			float t = (float)i / (float)points.Length;
			float ck = Curve(t);
			pos.x = Mathf.Lerp(begin.x, end.x, t) + u.x * ck;
			pos.y = Mathf.Lerp(begin.y, end.y, t) + u.y * ck;
			points[i] = pos;
		}
		
		Helper.SetActive(gameObject, true);
	}
	
	public Vector3 CurrentPos
	{
		get { return points[endIndex]; }
	}
	
	void Update ()
	{
		if(endIndex < points.Length-1)
		{
			++endIndex;
		}
		else if(beginIndex < points.Length-1)
		{
			++beginIndex;
		}
		else
		{
			Helper.SetActive(gameObject, false);
		}		
		
		UpdateVertices();
	}

}


















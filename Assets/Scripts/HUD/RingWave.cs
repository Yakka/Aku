using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class RingWave : MonoBehaviour 
{
	private const int NB_VERTICES = 24;
	private const float DURATION = 1f;
	private const float RADIUS_MIN = 10f;
	private const float RADIUS_MAX = 40f;
	private const float THICKNESS = 4f;
	
	private LineRenderer lineRenderer;
	private float startTime = -1;
	private Color color;
	
	void Awake()
	{
		name = "RingWave";
	}
	
	void Start ()
	{
		color = new Color(0.3f,0.3f,0.3f,0);
	}
	
	void UpdateVertices(float k)
	{
		if(lineRenderer == null)
		{
			lineRenderer = GetComponent<LineRenderer>();
			lineRenderer.material = new Material(Shader.Find("Mobile/Particles/Alpha Blended"));
			lineRenderer.SetColors(color, color);
			lineRenderer.SetWidth (THICKNESS, THICKNESS);
			lineRenderer.SetVertexCount(NB_VERTICES+1);
			lineRenderer.castShadows = false;
		}
		
		Vector3 center = transform.position;
		Vector3 pos = new Vector3(0,0,0);//HUD.Instance.transform.position;
		float r = (1f-k)*RADIUS_MIN + k*RADIUS_MAX;
		float t = 0;
		float t_step = 2f * Mathf.PI / (float)(NB_VERTICES);
		
		// Note : there is actually one more vertice in order to loop the line
		for (int i = 0; i <= NB_VERTICES; ++i)
		{
			pos.x = center.x + r * Mathf.Cos(t);
			pos.y = center.y + r * Mathf.Sin(t);
			lineRenderer.SetPosition(i, pos);
			t += t_step;
		}
		
		color.a = 1f - Helper.Sq(2f*k - 1f);
		lineRenderer.SetColors(color, color);
	}
	
	public void Spawn(float x, float y)
	{
		Helper.SetActive(gameObject, true);
		startTime = Time.time;
		UpdateVertices(0);
	}
	
	void Update ()
	{
		float k = (Time.time - startTime) / DURATION;
		if(k >= 0f && k <= 1f)
			UpdateVertices(k);
		else
		{
			startTime = -1;
			Helper.SetActive(gameObject, false);
		}
	}

}









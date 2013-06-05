using UnityEngine;
using System.Collections;

public class TmpRandomPainter : MonoBehaviour
{
	public GameObject paintStrip;
	private float angleRad;
	private float angleRadLastPlot;
	private float speed;
	private float crossedDistanceSinceLastPlot;
	
	void Start ()
	{
		speed = 30f;
	}
	
	private float GetPlotInterval()
	{
		float d = Helper.DeltaAngleRad(angleRadLastPlot, angleRad);
		if(Mathf.Abs(d) > Mathf.PI/16f)
			return 1f; // Closer points for more precision
		else
			return 2f; // Spaced points
	}
	
	void Update ()
	{
		angleRad += 5.0f*Mathf.PI * Time.deltaTime * (2f*Mathf.PerlinNoise(Time.time,0)-1f);
		Vector3 pos = transform.position;
		float s = speed * Time.deltaTime;
		pos.x += s * Mathf.Cos(angleRad);
		pos.y += s * Mathf.Sin(angleRad);
		transform.position = pos;
		
		crossedDistanceSinceLastPlot += s;
		if(crossedDistanceSinceLastPlot > GetPlotInterval())
		{
			float th = 0.2f + 0.8f * Mathf.PerlinNoise(2f*Time.time, 0);
			Color color = new Color(
				Mathf.PerlinNoise(2f*Time.time, 0),
				Mathf.PerlinNoise(2f*Time.time, 1),
				Mathf.PerlinNoise(2f*Time.time, 2), 1f);
			paintStrip.GetComponent<PolyPaintStrip>().Plot(pos, angleRad, th, color);
			crossedDistanceSinceLastPlot = 0;
			angleRadLastPlot = angleRad;
		}
	}

}









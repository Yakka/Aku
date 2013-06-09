using UnityEngine;
using System.Collections;

/// <summary>
/// Pen that creates progressive polygon-based paintings.
/// </summary>
public class PolyPainter 
{
	private const float WRAP_CUT_RADIUS = 100f;
	
	private Color color;
	private bool moonPaint;
	private PolyPaintStrip currentStrip;
	private Vector3 positionLastFrame = new Vector2();
	private float angleRad;
	private float angleRadLastPlot;
	private float crossedDistanceSinceLastPlot;
	private Vector3 stripBeginPos;
	
//	private static int instanceCount = 0;
//	public PolyPainter()
//	{
//		++instanceCount;
//		Debug.Log(instanceCount);
//	}
	
	public void Update(Vector3 pos)
	{
		angleRad = Helper.AngleRad(pos.x - positionLastFrame.x, pos.y - positionLastFrame.y);
		
		if(color.a > 0.02f)
		{
			if(currentStrip == null || currentStrip.Finished)
			{
				// Start a new strip
				NewStrip(pos);
			}
			
			float d = Vector3.Distance(pos, positionLastFrame);
			
			crossedDistanceSinceLastPlot += d;
			if(crossedDistanceSinceLastPlot > GetPlotInterval())
			{
				float th = 1f;
				currentStrip.Plot(pos, angleRad, th, color);
				crossedDistanceSinceLastPlot = 0;
				angleRadLastPlot = angleRad;

				if(Vector3.Distance(pos, stripBeginPos) > WRAP_CUT_RADIUS)
				{
					// Early strip cut to prevent too large level wrapping
					Finish();
				}
			}
			
			if(currentStrip.Finished)
			{
				// End strip, start a new one and link it to previous
				NewStrip(pos);
			}
		}
		else
		{
			Finish();
		}
		
		positionLastFrame = pos;
	}
	
	private void NewStrip(Vector3 startPos)
	{
		//Profiler.BeginSample("aaa");
		//++TmpDebug.Instance.newStripsPerFrame;
		
		PolyPaintStrip prevStrip = null;
		if(currentStrip != null)
			prevStrip = currentStrip;
		
		GameObject stripObj = new GameObject();
		stripObj.transform.position = startPos;
		stripObj.name = "generated_paint_strip";
		currentStrip = stripObj.AddComponent<PolyPaintStrip>();
		stripBeginPos = startPos;
		
		if(prevStrip != null)
			prevStrip.Next = currentStrip;
		
		//Profiler.EndSample();
	}
		
	public void Finish()
	{
		if(currentStrip != null && !currentStrip.Finished)
		{
			//Debug.Log("End");
			currentStrip.Finish();
			if(moonPaint)
				currentStrip.Fade();
		}
	}
	
	private float GetPlotInterval()
	{
		float d = Helper.DeltaAngleRad(angleRadLastPlot, angleRad);
		if(Mathf.Abs(d) > Mathf.PI/8f/*16f*/)
			return 2f;//1f; // Closer points for more precision
		else
			return 4f;//2f; // Spaced points
	}
	
//	private void SetColor(Color color)
//	{
//		SetColor(color, false);
//	}
//	
//	private void SetColor(Color newColor, bool isMoonPaint)
//	{
//		MoonPaint = isMoonPaint;
//		color = newColor;
//	}
	
	public bool MoonPaint
	{
		get { return moonPaint; }
		set
		{
			if(currentStrip != null && moonPaint && !value)
			{
				currentStrip.Finish();
				currentStrip.Fade();
			}
			moonPaint = value;
		}
	}
	
	public Color Color
	{
		get { return color; }
		set { color = value; }
	}

}






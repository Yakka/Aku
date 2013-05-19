using UnityEngine;
using System.Collections;

/// <summary>
/// Pen that creates progressive polygon-based paintings.
/// Currently does nothing.
/// </summary>
public class PolyPainter 
{
	//private Vector2 lastFramePosition;
	
	private Color color;
	private bool moonPaint;
	
	void Start () 
	{
	}
	
	void Update ()
	{
		// TODO Polygon-Quad-based painting
		//lastFramePosition = new Vector2(transform.position.x, transform.position.y);
	}
	
	void SetColor(Color color)
	{
		SetColor(color, false);
	}
	
	void SetColor(Color newColor, bool moonPaint)
	{
		this.moonPaint = moonPaint;
		color = newColor;
	}
	
	public bool MoonPaint
	{
		get { return moonPaint; }
	}

}




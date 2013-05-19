using UnityEngine;
using System.Collections;

/// <summary>
/// Paints stuff on the background by using SetPixel or shader-polygon methods
/// </summary>
public class PainterBehavior : MonoBehaviour 
{
	/// <summary>Optional material to link to the color of the paint</summary>
	public Material materialRef;
	
	public float distanceAutonomy = 200;
	
	private Color color;
	private float colorLevel; // in [0f,1f]
	private float crossedDistanceSinceColorCharge;
	private int colorIndex; // TODO use this in level 2
	
	private HiddenPainting hiddenPaintingRef;
	
	private PixelPainter pixelPainter = new PixelPainter();
	private PolyPainter polyPainter = null;
	
	private Vector3 positionLastFrame;
	
	//=================================================================
	
	void Start ()
	{
		//color = new Color(0,0,0,255);
		//pixelPainter = new PixelPainter();
		positionLastFrame = transform.position;
		colorIndex = 0;
	}
	
	public void SetBrush (float size)
	{
		if(pixelPainter != null)
			pixelPainter.SetBrush(size);
	}
	
	void Update ()
	{		
		float colorLevelK = 0;
		
		if(colorLevel > 0)
		{
			Vector3 pos = transform.position;
			
			float dd = Vector3.Distance(pos, positionLastFrame);
			crossedDistanceSinceColorCharge += dd;
			
			colorLevel = 1f - crossedDistanceSinceColorCharge / distanceAutonomy;
			if(colorLevel < 0)
				colorLevel = 0;
			else if(PolyPaintManager.Instance.decreaseCurve != null)
			{
				colorLevelK = Mathf.Clamp01(
					PolyPaintManager.Instance.decreaseCurve.Evaluate(colorLevel));
			}
			else
				colorLevelK = Mathf.Sqrt(colorLevel); // Sqrt makes a slow-start fast-end curve
			
			if(pixelPainter != null)
			{
				pixelPainter.color = color;
				pixelPainter.color.a *= colorLevelK;
				pixelPainter.Update(pos.x, pos.y);
			}
			
			// Check revealed zones
			if(hiddenPaintingRef != null)
			{
				hiddenPaintingRef.CheckReveal(pos.x, pos.y, colorIndex);
			}
		}
		
		if(materialRef != null)
		{
			Color matColor = color;
			matColor.r *= colorLevelK;
			matColor.g *= colorLevelK;
			matColor.b *= colorLevelK;
			materialRef.color = matColor;
		}
		
		positionLastFrame = transform.position;
	}
	
	void OnTriggerEnter(Collider other)
	{
		HiddenPainting hiddenPainting = null;		
		if((hiddenPainting = other.GetComponent<HiddenPainting>()) != null)
		{
			// Enter in a hidden painting zone
			if(pixelPainter != null)
			{
				hiddenPaintingRef = hiddenPainting;
				pixelPainter.hiddenPaintingRef = hiddenPainting;
			}
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		if(other.GetComponent<HiddenPainting>() != null)
		{
			// Exit a hidden painting zone
			if(pixelPainter != null)
			{
				hiddenPaintingRef = null;
				pixelPainter.hiddenPaintingRef = null;
			}
		}
	}
	
	public void SetColor(Color mixColor, bool charge)
	{
		SetColor(mixColor, charge, false);
	}
	
	public void SetColor(Color mixColor, bool charge, bool isMoonPaint)
	{
		color = mixColor;
		color.g *= 0.75f; // Because green is a bright color for the human eye
		
		if(pixelPainter != null)
		{
			pixelPainter.moonPaint = isMoonPaint;
			pixelPainter.color = color;
		}
		//Debug.Log("Picked color " + color);
		
		if(materialRef != null)
			materialRef.color = color;

		// TODO subtractive color mix

		if(charge)
		{
			crossedDistanceSinceColorCharge = 0;
			positionLastFrame = transform.position;
			colorLevel = 1;
		}
	}
	
	public Color Color
	{
		get { return color; }
	}
	
	public float ColorLevel
	{
		get { return colorLevel; }
		set 
		{ 
			colorLevel = Mathf.Clamp(value, 0f, 1f); 
			crossedDistanceSinceColorCharge = distanceAutonomy*(1f-colorLevel);
			positionLastFrame = transform.position;
		}
	}
	
	public bool HasColor
	{
		get { return colorLevel > 0.001f; }
	}
	
	public bool HasMoonPaint
	{
		get 
		{
			if(pixelPainter != null)
				return pixelPainter.moonPaint;
			else if(polyPainter != null)
				return polyPainter.MoonPaint;
			else
				return false;
		}
	}

}

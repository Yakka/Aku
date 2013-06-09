using UnityEngine;
using System.Collections;

/// <summary>
/// Paints stuff on the background by using SetPixel or shader-polygon methods
/// </summary>
public class PainterBehavior : MonoBehaviour 
{
	/// <summary>Optional material to link to the color of the paint</summary>
	public Material optionalMaterialRef;
	
	public float distanceAutonomy = 200;
	public bool affectedByClouds = true; // Color recharge when touching a cloud or the moon
	
	private Color color;
	private float colorLevel; // in [0f,1f]
	private float crossedDistanceSinceColorCharge;
	private int colorIndex;
	
	private HiddenPainting hiddenPaintingRef;
	
	// Note : there is two linear paint systems, 
	// directly with pixels or with polygons.
	// Only one should be assigned, the other must be null.
	private PixelPainter pixelPainter = null; //new PixelPainter();
	private PolyPainter polyPainter = new PolyPainter();
	
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
			if(dd > 100f)
			{
				dd = 0;
				positionLastFrame = pos;
			}
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
			if(polyPainter != null)
			{
				Color pcolor = color;
				pcolor.a *= colorLevelK;
				polyPainter.Color = pcolor;
				polyPainter.Update(new Vector2(pos.x, pos.y));
			}
			
			// Check revealed zones
			if(hiddenPaintingRef != null)
			{
				hiddenPaintingRef.CheckReveal(pos.x, pos.y, colorIndex);
			}
		}
		else
		{
			if(polyPainter != null)
				polyPainter.Finish();
		}
		
		if(optionalMaterialRef != null)
		{
			Color matColor = color;
			matColor.r *= colorLevelK;
			matColor.g *= colorLevelK;
			matColor.b *= colorLevelK;
			optionalMaterialRef.color = matColor;
		}
		
		positionLastFrame = transform.position;
	}
	
	public void FinishStrip()
	{
		if(polyPainter != null)
			polyPainter.Finish();
	}
	
	void PreLevelWrap()
	{
	}
	
	void PostLevelWrap()
	{
		//Debug.Log(gameObject.name + ": PostLevelWrap");
		positionLastFrame = transform.position;
		FinishStrip();
	}
	
	void OnTriggerEnter(Collider other)
	{
		HiddenPainting hiddenPainting = null;
		Cloud cloud = null;
		
		if((hiddenPainting = other.GetComponent<HiddenPainting>()) != null)
		{
			// Enter in a hidden painting zone
			if(pixelPainter != null)
			{
				hiddenPaintingRef = hiddenPainting;
				pixelPainter.hiddenPaintingRef = hiddenPainting;
			}
		}
		else if(affectedByClouds)
		{
			if((cloud = other.GetComponent<Cloud>()) != null)
			{
				// Set color but not charged yet
				if(Level.Get.levelID == 2)
					SetColor(cloud.colorIndex, false);
				else
					SetColor(cloud.CurrentColor, false, false);
			}
			else if(other.name == "Moon")
			{
				// Set moon color but not charged yet
				SetColor(Color.white, false, true);
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
		else if(affectedByClouds)
		{
			if(other.GetComponent<Cloud>() != null || other.name == "Moon")
			{
				ColorLevel = 1f; // Set color as charged
			}
		}
	}
	
	/// <summary>
	/// Sets the color of the painter by using a color index.
	/// Color indexes must be defined in the global paint manager.
	/// In levels involving tricolor paint reveal, 0, 1 and 2
	/// will be converted into red, blue and green because the final
	/// color may be computed in a shader. Other indexes will create moon paint.
	/// </summary>
	/// <param name='index'>Color index.</param>
	/// <param name='charge'>Does the painter have to recharge paint load too?</param>
	public void SetColor(int index, bool charge)
	{
		colorIndex = index;
		if(Level.Get.levelID == 2)
		{
			// In level 2, the final color comes into the reveal shader.
			// So we use the three components as the only 3 allowed pseudo-colors.
			// Moon paint is still available but for use in space only.
			switch(colorIndex)
			{
			case 0: SetColor(Color.red, charge); break;
			case 1: SetColor(Color.green, charge); break;
			case 2: SetColor(Color.blue, charge); break;
			default: SetColor(Color.white, charge, true); break;
			}
		}
		else
		{
			// In other levels, standard color indexing can be used (if specified).
			if(index >= 0 || index < PolyPaintManager.Instance.indexedColors.Length)
				SetColor(PolyPaintManager.Instance.indexedColors[index], charge);
			else
				SetColor(Color.white, charge, true);
		}
	}
	
	public void SetColor(Color mixColor, bool charge)
	{
		SetColor(mixColor, charge, false);
	}
	
	public void SetColor(Color mixColor, bool charge, bool isMoonPaint)
	{
		color = mixColor;
		/*if(!isMoonPaint)
		{
			// Reduces brightness a bit, looks more "painty" (especially for yellow),
			// and also Because green is a bright color for the human eye
			color.g *= 0.75f;
		}*/
		
		if(pixelPainter != null)
		{
			pixelPainter.moonPaint = isMoonPaint;
			pixelPainter.color = color;
		}
		if(polyPainter != null)
		{
			polyPainter.MoonPaint = isMoonPaint;
			polyPainter.Color = color;
		}
		//Debug.Log("Picked color " + color);
		
		if(optionalMaterialRef != null)
			optionalMaterialRef.color = color;

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
	
	public int ColorIndex
	{
		get { return colorIndex; }
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
				return pixelPainter.moonPaint && HasColor;
			else if(polyPainter != null)
				return polyPainter.MoonPaint && HasColor;
			else
				return false;
		}
	}

}

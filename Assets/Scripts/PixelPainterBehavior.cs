using UnityEngine;
using System.Collections;

/// <summary>
/// Something that paints stuff on the level background
/// (not only the drake, it can be anything else)
/// NO LONGER UPDATED : switch to PainterBehavior
/// </summary>
public class PixelPainterBehavior : MonoBehaviour
{	
	/// <summary>Optional material to link to the color of the paint</summary>
	public Material materialRef;
	
	public float distanceAutonomy = 200;
	
	private Color color;
	private float colorLevel; // in [0f,1f]
	private float crossedDistanceSinceColorCharge;
	private PixelPainter pixelPainter = new PixelPainter();
	
	private Vector3 positionLastFrame;
	
	//=================================================================
	
	void Start ()
	{
		//color = new Color(0,0,0,255);
		//pixelPainter = new PixelPainter();
		positionLastFrame = transform.position;
		
		Debug.LogError("PixelPainterBehavior is outdated," +
		 	"Consider using PainterBehavior instead. ");
	}
	
	public void SetBrush (float size)
	{
		pixelPainter.SetBrush(size);
	}
	
	void Update ()
	{		
		float colorLevelK = 0;
		
		if(colorLevel > 0)
		{
			float dd = Vector3.Distance(transform.position, positionLastFrame);
			crossedDistanceSinceColorCharge += dd;
			
			colorLevel = 1f - crossedDistanceSinceColorCharge / distanceAutonomy;
			if(colorLevel < 0)
				colorLevel = 0;
			else
				colorLevelK = Mathf.Sqrt(colorLevel); // Sqrt makes a slow-start fast-end curve
			
			pixelPainter.color = color;
			pixelPainter.color.a *= colorLevelK;
			pixelPainter.Update(transform.position.x, transform.position.y);
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
			pixelPainter.hiddenPaintingRef = hiddenPainting;
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		if(other.GetComponent<HiddenPainting>() != null)
		{
			// Exit a hidden painting zone
			pixelPainter.hiddenPaintingRef = null;
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
		
		pixelPainter.moonPaint = isMoonPaint;
		pixelPainter.color = color;
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
	
}

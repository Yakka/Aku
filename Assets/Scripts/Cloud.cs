using UnityEngine;
using System.Collections;

/// <summary>
/// A paint ball.
/// </summary>
public class Cloud : MonoBehaviour
{
	// In level 2, colors are only display stuff and are not 
	// linked to the actually painted color.
	public Color color;
	public Color enlightColor;
	
	public int colorIndex; // For use in level 2.
	
	private Transform sprite;
	private bool enlighted = false;
	
	void Start ()
	{
		// Set appropriate color to the cloud
		sprite = transform.GetChild(0);		
		UpdateSpriteColor();
	}
	
	void UpdateSpriteColor()
	{
		// Note : the material is assumed to be "VertexLit blended"
		// TODO Color vertices instead, more efficient
		sprite.renderer.materials[0].SetColor("_EmisColor", CurrentColor);
	}
	
	public Color CurrentColor
	{
		get 
		{ 
			return enlighted ? enlightColor : color;
		}
	}
	
	public void SwapColors(Cloud other)
	{
		// Actually, the objects don't change their positions,
		// but their appearance.
		
		Color temp = color;
		color = other.color;
		other.color = temp;
		
		temp = enlightColor;
		enlightColor = other.enlightColor;
		other.enlightColor = temp;
		
		int temp2 = colorIndex;
		colorIndex = other.colorIndex;
		other.colorIndex = temp2;

		UpdateSpriteColor();
		other.UpdateSpriteColor();
		//Debug.Log("Cloud.SwapColors : " + temp + " -> " + color);
	}
	
	public bool Enlighted
	{
		get 
		{
			return enlighted;
		}
		
		set
		{
			if(value != enlighted)
			{
				enlighted = value;
				UpdateSpriteColor();
			}
		}
	}

}



using UnityEngine;
using System.Collections;

public class Cloud : MonoBehaviour
{
	public Color color;
	public Color enlightColor;
	//public Material baseMaterial;
	
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
		get { return enlighted ? enlightColor : color; }
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



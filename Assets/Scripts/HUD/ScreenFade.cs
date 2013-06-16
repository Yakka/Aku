using UnityEngine;
using System.Collections;

public class ScreenFade : MonoBehaviour 
{
	private static Texture2D fillTexture;
	private static ScreenFade globalInstance;
	
	private Color color;
	private float beginTime;
	private float endTime;
	private bool isFadeIn;
	private Rect rect;
	
	void Awake() { globalInstance = this; }
	public static ScreenFade Instance { get { return globalInstance; } }
	
	void Start ()
	{
		if(fillTexture == null)
		{
			Color fillColor = new Color(1f,1f,1f,1f);
			fillTexture = new Texture2D(1,1, TextureFormat.ARGB32, false, false);
			fillTexture.SetPixel(0,0, fillColor);
			fillTexture.Apply();
		}
		
		rect = new Rect(0, 0, Screen.width, Screen.height);
		color = new Color(1f, 1f, 1f, 1f);
		
		Fade(2f, true);
		//Helper.SetActive(gameObject, false);
	}
	
	void Fade(float duration, bool fadeIn)
	{
		beginTime = Time.time;
		endTime = beginTime + duration;
		isFadeIn = fadeIn;
		//Helper.SetActive(gameObject, true);
	}
	
	public bool IsFading
	{
		get { return Time.time >= beginTime && Time.time < endTime; }
	}
	
	void Update ()
	{
		float t = Time.time;
		if(IsFading)
		{
			float k = (t - beginTime) / (endTime - beginTime);
			if(isFadeIn)
				color.a = 1f - k;
			else
				color.a = k;
		}
//		else
//		{
//			Helper.SetActive(gameObject, false);
//		}
	}
	
	void OnGUI()
	{
		if(IsFading)
		{
			GUI.color = color;
			GUI.DrawTexture(rect, fillTexture);
		}
	}

}





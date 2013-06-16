using UnityEngine;
using System.Collections;

public class ScreenFade : MonoBehaviour 
{
	private static Texture2D blackTexture;
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
		if(blackTexture == null)
		{
			Color black = new Color(0,0,0,1);
			blackTexture = new Texture2D(1,1, TextureFormat.ARGB32, false, false);
			blackTexture.SetPixel(0,0, black);
			blackTexture.Apply();
		}
		
		rect = new Rect(0, 0, Screen.width, Screen.height);
		
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
			GUI.DrawTexture(rect, blackTexture);
		}
	}

}





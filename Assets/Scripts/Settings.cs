using UnityEngine;
using System.Collections;

public class Settings : MonoBehaviour
{
	/// <summary>
	/// This option enables or disables tablet-specific behavior of the game.
	/// I use it for testing on my desktop computer.
	/// Must be set to true before each tablet export.
	/// </summary>
	public static bool onTablet = false;
	
	// Enable the trailer mode
	public static bool trailerMode = false;
	
	/// <summary>
	/// Enables debug extra-functionnalities that should not be visible in the end-user game.
	/// </summary>
	public static bool debugMode = false;
	
	public static bool releaseMode = false;
	
	public static GUIStyle debugGuiStyle;
	
	void Awake()
	{
		Resolution[] res = Screen.GetResolution;
		Debug.Log("Supported resolutions :");
		for(int i = 0; i < res.Length; ++i)
		{
			Debug.Log(res[i].width + "x" + res[i].height);
		}
		//Screen.SetResolution(Screen.width, Screen.height, true);
	}
	
	void Start () 
	{
		QualitySettings.antiAliasing = 4;
		
		debugGuiStyle = new GUIStyle();
		debugGuiStyle.normal.textColor = Color.red;
		
		if(Application.platform == RuntimePlatform.IPhonePlayer)
		{
			// TODO do something about the timescale because first versions of iPad are sloowww
		}
	}
	
	void OnGUI()
	{
		if(!trailerMode || releaseMode)
		{
			if(GUI.Button(new Rect(10, 10, 100, 24), debugMode ? "Debug[ON]" : "Debug[OFF]"))
			{
				Debug.Log("Debug mode toggle");
				debugMode = !debugMode;
			}
		}

		//GUI.Label(new Rect (fingerX, fingerY, fingerSize, fingerSize), fingerTexture);
		
	}
	
}

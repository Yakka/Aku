using UnityEngine;
using System.Collections;

/// <summary>
/// Hidden camera handler.
/// 1) Create a new camera
/// 2) Set its layer to HiddenPainting only
/// 3) Sets its depth < depth of main camera so that it will be renderer before
/// 4) Attach this script to the camera
/// </summary>
public class HiddenCameraHandler : MonoBehaviour 
{
	public RenderTexture renderTarget;
	
	void Start()
	{
		if(camera == null)
		{
			Debug.LogError("HiddenCameraHandler NOT attached to a camera !");
		}
		
		// Do not use this because it copies everything, not only what we need
		//camera.CopyFrom(Camera.mainCamera);
		
		//renderTarget = new RenderTexture((int)camera.pixelWidth, (int)camera.pixelHeight, 16);
		renderTarget = new RenderTexture(Screen.width, Screen.height, 16);
		
		int mask = LayerMask.NameToLayer("HiddenPainting");
		//Debug.Log("HiddenPainting layer: " + mask);
		gameObject.layer = mask;
		camera.cullingMask = 1 << mask;
		camera.depth = Camera.mainCamera.depth - 1;
		camera.targetTexture = renderTarget;
		camera.backgroundColor = Color.black;
		
		// Global texture of the screen containing only hidden paintings
		Shader.SetGlobalTexture("_HiddenPaint", camera.targetTexture);
	}
	
	void Update () 
	{
		// This camera is sticked to the main camera
		transform.position = Camera.mainCamera.transform.position;
		camera.orthographicSize = Camera.mainCamera.orthographicSize;
	}
	
	void OnGUI()
	{
		if(Settings.debugMode)
		{
			GUI.Label(new Rect(16, 120, 256, 32), 
				"Hidden world renderTexture size : " + renderTarget.width + ", " + renderTarget.height,
				Settings.debugGuiStyle);			
			
		}
	}

//	void OnPostRender()
//	{
//	}

}



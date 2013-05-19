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
	
	void Awake()
	{
		if(camera == null)
		{
			Debug.LogError("HiddenCameraHandler NOT attached to a camera !");
		}
		
		// Do not use this because it copies everything, not only what we need
		//camera.CopyFrom(Camera.mainCamera);
		
		renderTarget = new RenderTexture((int)camera.pixelWidth, (int)camera.pixelHeight, 16);
		
		int mask = LayerMask.NameToLayer("HiddenPainting");
		//Debug.Log("HiddenPainting layer: " + mask);
		gameObject.layer = mask;
		camera.cullingMask = 1 << mask;
		camera.depth = Camera.mainCamera.depth - 1;
		camera.targetTexture = renderTarget;
		camera.backgroundColor = Color.black;
	}
	
	void Update () 
	{
		// This camera is sticked to the main camera
		transform.position = Camera.mainCamera.transform.position;
		camera.orthographicSize = Camera.mainCamera.orthographicSize;
	}

	void OnPostRender()
	{
		// Global texture of the screen containing only hidden paintings
		Shader.SetGlobalTexture("_HiddenPaint", camera.targetTexture);
	}

}



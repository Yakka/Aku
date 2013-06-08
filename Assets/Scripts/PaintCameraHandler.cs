using UnityEngine;
using System.Collections;

public class PaintCameraHandler : MonoBehaviour 
{	
	public RenderTexture renderTarget;
	public int resolutionDivider = 2;
	
	private static PaintCameraHandler globalInstance;
	
	void Awake() { globalInstance = this; }
	public static PaintCameraHandler Instance { get { return globalInstance; } }
	
	void Start ()
	{
		CheckStuff();
		
		renderTarget = new RenderTexture(
			Screen.width/resolutionDivider, 
			Screen.height/resolutionDivider, 16);
		
		int mask = LayerMask.NameToLayer("PaintReveal");
		//Debug.Log("HiddenPainting layer: " + mask);
		gameObject.layer = mask;
		camera.cullingMask = 1 << mask;
		camera.depth = Camera.mainCamera.depth - 1;
		camera.targetTexture = renderTarget;
		camera.backgroundColor = new Color(0,0,0,0);
		
		// Global texture of the screen containing only hidden paintings
		Shader.SetGlobalTexture("_RevealPaint", camera.targetTexture);
	}
	
	void Update () 
	{
		// This camera is sticked to the main camera
		transform.position = Camera.mainCamera.transform.position;
		camera.orthographicSize = Camera.mainCamera.orthographicSize;
	}
	
	void CheckStuff()
	{
		if(camera == null)
		{
			Debug.LogError("PaintCameraHandler NOT attached to a camera !");
		}
		if(resolutionDivider == 0)
		{
			resolutionDivider = 1;
			Debug.LogError(name + ": resolutionDivider invalid value (0), auto set to 1 !");
		}
		if(renderTarget != null)
		{
			Debug.LogError(name + ": please leave renderTarget empty (created from script), " +
				"it is visible in the editor to make it viewable ingame.");
		}
	}

}

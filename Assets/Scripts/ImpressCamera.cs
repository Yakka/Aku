using UnityEngine;
using System.Collections;

/// <summary>
/// The game object of this camera must be attached to a tile,
/// and renders stuff on its paint layer using accelerated graphics.
/// 
/// 1) Activate 2 cameras : one for the hidden world, the other for reveal
/// 
/// 2) Render a small chunk of the hidden world, on a texture the size of the tile.
///    I do this instead of picking the global screen texture, because it could occur
///    even if the main camera doesn't sees the tile we are rendering.
/// 
/// 3) Bind that texture on a reveal shader and render paintings using it 
///    on another texture having the same size (this one will be displayed over the tile).
/// 
/// 4) Disable the 2 cameras
/// 
/// </summary>
public class ImpressCamera : MonoBehaviour 
{
	private const int RENDER_WIDTH = Tile.PAINT_TEXTURE_WIDTH;
	private const int RENDER_HEIGHT = Tile.PAINT_TEXTURE_HEIGHT;
	private const float RESOLUTION_COEFF = 1; // Must be 1 at the moment !
	
	private static Material clearMaterial;

	private GameObject hiddenCamObj;
	private RenderTexture hiddenRenderTarget;
	private RenderTexture renderTarget;
	private bool takeRequested;
	private Tile tileRef;
	private Material revealMaterial;
	private bool isFirstRender = true;
			
	public void Setup(Tile tile)
	{
		if(tileRef != null)
		{
			Debug.LogError(name + ": Setup called two times, aborting !");
			return;
		}
		
		if(clearMaterial == null)
		{
			clearMaterial = Helper.CreateMaterial("Clearer");
		}
		
		tileRef = tile;
		
		Helper.SetActive(gameObject, false);
		
		// Create camera
		Camera cam = null;
		if(camera == null)
			cam = gameObject.AddComponent<Camera>();
		// Camera mask
		int mask = LayerMask.NameToLayer("Impress");
		gameObject.layer = mask;
		cam.cullingMask = 1 << mask;
		// Other camera settings
		cam.depth = Camera.mainCamera.depth - 1;
		cam.backgroundColor = new Color(0,0,0,0);
		cam.aspect = 1f;
		cam.orthographic = true;
		cam.orthographicSize = Tile.SIZE/2; // A half-size is expected
		
		// Create hidden camera
		hiddenCamObj = new GameObject();
		hiddenCamObj.transform.parent = this.transform.parent;
		Camera hcam = hiddenCamObj.AddComponent<Camera>();
		// Hidden camera mask
		int hmask = LayerMask.NameToLayer("HiddenPainting");
		hiddenCamObj.layer = hmask;
		hcam.cullingMask = 1 << hmask;
		// Other hidden camera settings
		hcam.depth = cam.depth - 1; // It will render just before the impress camera
		hcam.backgroundColor = new Color(0,0,0,0);
		hcam.aspect = 1f;
		hcam.orthographic = true;
		hcam.orthographicSize = Tile.SIZE/2;
		
		// Create render targets
		hiddenRenderTarget = CreateRenderTarget();
		hcam.targetTexture = hiddenRenderTarget;
		
		renderTarget = CreateRenderTarget();
		cam.targetTexture = renderTarget;

		// Get reveal material
		if(revealMaterial == null)
		{
			if(Level.Get.levelID == 2)
				revealMaterial = Helper.CreateMaterial("TriPaintReveal");
			else
				revealMaterial = Helper.CreateMaterial("PaintReveal");
			//revealMaterial = Helper.CreateMaterial("Mobile/Particles/Alpha Blended");
		}
	}
	
//	void Start ()
//	{
//	}
	
	private RenderTexture CreateRenderTarget()
	{
		RenderTexture rt = new RenderTexture(
			(int)((float)RENDER_WIDTH * RESOLUTION_COEFF), 
			(int)((float)RENDER_HEIGHT * RESOLUTION_COEFF), 24);
		return rt;
		//camera.targetTexture = renderTarget;
	}
	
	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		//Debug.Log(name + ": shot " + tileRef);
		
		if(isFirstRender)
		{
			// I do this because Unity seems to have an issue about RenderTextures :
			// They display garbage pixels, as if the texture was not properly initialized.
			// So this clears the destination on the first rendering.
			
			//GL.Clear(true, true, Color.clear);
			Graphics.Blit(source, destination, clearMaterial);
			isFirstRender = false;
		}
		
		Graphics.Blit(source, destination, revealMaterial);
		
		takeRequested = false;
		
		tileRef.PostPaintImpress();
		//Helper.SetActive(gameObject, false);
	}
	
	public void RequestTake()
	{
		if(!takeRequested)
		{
			takeRequested = true;
			Helper.SetActive(gameObject, true);
			Helper.SetActive(hiddenCamObj, true);
			
			// This camera must be at the center of the tile
			// Note : it assumes the tile position is on its bottom-left corner
			Vector3 pos = tileRef.transform.position;
			pos.z = -100f;
			pos.x += Tile.SIZE / 2;
			pos.y += Tile.SIZE / 2;
			this.transform.position = pos;
			hiddenCamObj.transform.position = pos;
			
			revealMaterial.SetTexture("_HiddenPaint", hiddenRenderTarget);
			revealMaterial.SetVector("_HiddenScale", new Vector4(1,-1,1,1));
		}
	}
	
	void Update () 
	{
		//Debug.Log(name + ": update " + tileRef);
		if(!takeRequested)
		{
			Helper.SetActive(gameObject, false);
			Helper.SetActive(hiddenCamObj, false);
			return;
		}
	}
	
	public RenderTexture RenderTarget
	{
		get { return renderTarget; }
	}

}




using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents a world tile.
/// It can reference objects that are attached to it.
/// It will also recognize several types of children from their name :
/// "bg" : the tile background
/// "paintLayer" : will make the tile paintable
/// </summary>
public class Tile : MonoBehaviour
{
	// This is the scale of each tile in world units (not pixels !)
	public const int SIZE = 64; 
	
	public const int PAINT_TEXTURE_WIDTH = 128;
	public const int PAINT_TEXTURE_HEIGHT = 128;
	
	private static Color whiteAlpha = new Color(1f, 1f, 1f, 0f);
	// pre-computed buffer for constructing the paintTexture
	private static Color[] alphaPaintBuffer; 
	
	// Config
	public bool water;
	public bool space;
	
	// Objects linked to this tile (only public for debug !)
	public ArrayList objects = new ArrayList(); 
	
	private bool initialized = false;	
	private int gridPosX; // In tiles (remains constant !)
	private int gridPosY; // In tiles (remains constant !)
	
	private Texture paintTexture; // Texture used for pixellized paintings
	private Transform paintLayer;
	//private bool pixelsWereModified = false;
	
	private GameObject impressCameraObj;
	
	//private Hashtable fadingPixels = new Hashtable();
	
	void Start ()
	{
		initialized = true;
		
		// Disable paintLayer, it will be activated later
		paintLayer = this.transform.FindChild("paintLayer");
		if(paintLayer != null)
			Helper.SetActive(paintLayer.gameObject, false);
		
		// Build pre-computed alpha paint
		if(alphaPaintBuffer == null)
		{
			alphaPaintBuffer = new Color[PAINT_TEXTURE_WIDTH*PAINT_TEXTURE_HEIGHT];
			for(uint i = 0; i < alphaPaintBuffer.Length; ++i)
			{
				alphaPaintBuffer[i] = whiteAlpha;
			}
		}
	}
	
	public void PreLevelWrap()
	{
		foreach(GameObject obj in objects)
		{
			obj.BroadcastMessage("PreLevelWrap", 
				SendMessageOptions.DontRequireReceiver);
		}
	}

	public void PostLevelWrap()
	{
		foreach(GameObject obj in objects)
		{
			obj.BroadcastMessage("PostLevelWrap", 
				SendMessageOptions.DontRequireReceiver);
		}
	}
	
	/*void Update ()
	{
		// This is related to the old pixel-paint system
//		if(paintTexture != null)
//		{
//			UpdateFadingPixels();
//			if(pixelsWereModified) // If we manually changed the paint texture
//			{
//				paintTexture.Apply(); // Send texture to graphic card for update
//				pixelsWereModified = false;
//			}
//		}
	}*/
		
	public void WorldToTextureCoords(float worldX, float worldY, out int texX, out int texY)
	{
		float tx = worldX / (float)SIZE;
		float ty = worldY / (float)SIZE;
		// Castcastcastcastcastcastcast
		texX = (int)((float)PAINT_TEXTURE_WIDTH * (tx - Mathf.Floor(tx))); 
		texY = (int)((float)PAINT_TEXTURE_HEIGHT * (ty - Mathf.Floor(ty)));
	}
	
	public void Attach(GameObject obj)
	{
		objects.Add(obj);
	}
	
	public bool Detach(GameObject obj)
	{
		if(objects.Contains(obj))
		{
			objects.Remove(obj);
			return true;
		}
		return false;
	}
	
	public void Move(int tdx, int tdy)
	{
		float dx = tdx * SIZE;
		float dy = tdy * SIZE;
		
		// Move the tile
		Vector3 pos = transform.position;
		pos.x += dx;
		pos.y += dy;
		transform.position = pos;

		// Move objects
		foreach(GameObject obj in objects)
		{
			pos = obj.transform.position;
			pos.x += dx;
			pos.y += dy;
			obj.transform.position = pos;
		}
	}
	
	public void SetGridPos(int tx, int ty)
	{
		if(initialized)
			Debug.LogWarning("Tile pos set but was already initialized");
		gridPosX = tx;
		gridPosY = ty;
	}
	
	public override string ToString()
	{
		return "Tile(" + gridPosX + ", " + gridPosY 
			+ ", objs:" + objects.Count 
			+ ", paintable=" + IsPaintable 
			+ ", painted=" + IsPainted + ")";
	}
	
	public void RequestPaintImpress(GameObject obj)
	{
		if(paintLayer == null)
			return;
		
		ImpressCamera camScript = null;
		
		if(impressCameraObj != null)
		{
			camScript = impressCameraObj.GetComponent<ImpressCamera>();
		}
		else
		{
			impressCameraObj = new GameObject();
			impressCameraObj.name = "impress_camera";
			impressCameraObj.transform.parent = this.transform;
			
			// Add camera script
			camScript = impressCameraObj.AddComponent<ImpressCamera>();
			camScript.Setup(this);
			paintTexture = camScript.RenderTarget;
			
			// Set material on the paint texture
			Material mat = Helper.CreateMaterial("Mobile/Particles/Alpha Blended");
			mat.mainTexture = paintTexture;
			paintLayer.renderer.material = mat;
		}
		
		//Level.Get.impressManager.PreImpress(obj, new Vector2i(gridPosX, gridPosY));
		camScript.RequestTake();
	}
	
	public void PostPaintImpress()
	{
		//Level.Get.impressManager.PostImpress(new Vector2i(gridPosX, gridPosY));
		
		// Ensure the paintlayer is active in order to display it
		Helper.SetActive(paintLayer.gameObject, true);
	}
	
	public bool IsWater { get { return water; } }	
	public bool IsSpace { get { return space; } }
	public bool IsPaintable  { get { return paintLayer != null; } }
	public bool IsPainted    { get { return paintTexture != null; } }
	public int PosX { get { return gridPosX; } }
	public int PosY { get { return gridPosY; } }
	
	
	#region "Legacy pixel-paint methods"
	//===============================================================================
	/// LEGACY PIXEL PAINTING //////////////////////
	//===============================================================================
	
	/*
	public Color GetPaintPixel(int texX, int texY)
	{
		if(paintTexture == null)
			return Color.clear;
		return paintTexture.GetPixel(texX, texY);
	}
	*/

	/*
	private void GeneratePaintTexture()
	{
		//Debug.Log("Creating paint texture for tile (" + gridPosX + ", " + gridPosY + ")");
		// Generate the paint texture (4 bytes per pixel without mipmaps and using linear filtering
		// Note : I would like to use ARGB4444 (16-bit) but it is not supported...
		paintTexture = new Texture2D(
			PAINT_TEXTURE_WIDTH, PAINT_TEXTURE_HEIGHT,
			TextureFormat.RGBA32, false, true);
		
		paintTexture.wrapMode = TextureWrapMode.Clamp;
		
		// Initialize to alpha
		paintTexture.SetPixels(alphaPaintBuffer);
		
		//Material mat = new Material(Shader.Find("Mobile/Particles/Alpha Blended"));
		Material mat = new Material(PolyPaintManager.Instance.PixelPaintMaterial);
		mat.SetTexture("_MainTex", paintTexture);
		paintLayer.gameObject.renderer.material = mat;
		//Helper.SetQuadMeshColors(paintLayer.gameObject, Color.white);
		
		paintLayer.gameObject.active = true;
	}
	*/
	
	/*
	/// <summary>
	/// Sets a pixel of the paint layer if available.
	/// </summary>
	/// <param name='color'>Pixel color.</param>
	/// <param name='texX'>Tex x.</param>
	/// <param name='texY'>Tex y.</param>
	/// <param name='fadeDelay'>
	/// If < 0, no fading. If 0, immediate fading. If > 0, delayed fading.
	/// </param>
	public void SetPaintPixel(Color color, int texX, int texY, float fadeDelay, float fadeSpeed)
	{
		if(paintLayer == null)
			return; // This tile doesn't supports painting
		
		// The paint texture is created when it is really needed
		if(paintTexture == null) 
			GeneratePaintTexture();
		
		if(texX >= 0 && texY >= 0 && texX < paintTexture.width && texY < paintTexture.height)
		{
			paintTexture.SetPixel(texX, texY, color);
			pixelsWereModified = true;
			
			if(fadeDelay >= 0)
			{
				Vector2i pos = new Vector2i(texX, texY);
				if(!fadingPixels.Contains(pos))
				{
					fadingPixels.Add(pos, new FadingPixel(fadeDelay, fadeSpeed));
				}
			}
		}
	}
	*/
	/*
	public void SetPaintPixelNoFade(Color color, int texX, int texY)
	{
		if(paintLayer == null)
			return; // This tile doesn't supports painting
		
		// The paint texture is created when it is really needed
		if(paintTexture == null)
			GeneratePaintTexture();
		
		if(texX >= 0 && texY >= 0 && texX < paintTexture.width && texY < paintTexture.height)
		{
			paintTexture.SetPixel(texX, texY, color);
			pixelsWereModified = true;
		}
	}
	*/
	/*
	void UpdateFadingPixels()
	{
		// Update fading pixels (invisible ink)
		if(fadingPixels.Count > 0) // If there is fading pixels
		{
			ArrayList fadedPixels = new ArrayList();
			//Debug.Log("fp: " + fadingPixels.Count + "---" + Time.time);
			
			//Profiler.BeginSample("Pixel fade");
			foreach(DictionaryEntry e in fadingPixels) // For each fading pixel
			{
				FadingPixel fp = (FadingPixel)e.Value; // Back to Java style???
				
				if(Time.time-fp.time > fp.delay) // If the delay is reached
				{
					Vector2i pos = (Vector2i)e.Key;
					Color pix = paintTexture.GetPixel(pos.x, pos.y); // Get the pixel

					// 50% overhead is due to the loop.
					// Since pixelFadings are created in an increasing timestamp,
					// use a queue-like delaying system.
					
					// IMPORTANT NOTE: using pix.a '>' sometimes prevents entering the code below...
					// So I written '>=' instead.
					// That's how I (somewhat) fixed a massive lag caused by endless PixelFadings.
					if(pix.a >= 0) // If the pixel is not dead yet
					{
						// Reduce pixel alpha to make it disappear
						pix.a -= fp.speed * Time.deltaTime; 
						
						if(pix.a <= 0) // If the pixel is dead
						{
							pix.a = 0;
							fadedPixels.Add(pos); // Its fading should be removed
						}
						paintTexture.SetPixel(pos.x, pos.y, pix);
						pixelsWereModified = true;
					}
				}
			}
			//Profiler.EndSample();
			// Remove dead pixels from fade list
			foreach(Vector2i pos in fadedPixels)
			{
				fadingPixels.Remove(pos);
			}
		}
	}
	*/
	
	#endregion

}


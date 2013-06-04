using UnityEngine;
using System.Collections;

public class CameraHandler : MonoBehaviour 
{
	//private float startZ;
	private const float FOLLOW_SPEED_AIR = 1.5f;
	private const float FOLLOW_SPEED_SPACE = 1.5f;
	private const float FOLLOW_SPEED_SEA = 1.5f;
	private const float SPEED_AIR_TO_SEA = 0.5f; // Used for slowing the camera when entering into the sea
	private const float AIR_SIZE = 60f; // Field of view in the air
	private const float SEA_SIZE = 50f; // Filed of view in the sea
	private const float SIZE_TRANSITION = 0.002f;
	
	/// <summary>
	/// The target the camera must follow.
	/// </summary>
	public Transform target;
	
	public GameObject levelObj;
	public float airShift; // 0 disable the shifting

	private Drake drakeRef; // Only used when we really have to access drake's properties
	private int tilePosX;
	private int tilePosY;
	private int lastFrameTilePosX;
	private int lastFrameTilePosY;
	private bool isFirstUpdate;
	private float shakeAmp;
	private float shakeDuration;
	private float currentSpeed;
	private float currentSize;
	
	private Level level;
	
	

	// Use this for initialization
	void Start () 
	{
		//startZ = this.transform.position.z;
		
		// Get level script
		level = levelObj.GetComponent("Level") as Level;
		
		drakeRef = target.GetComponent<Drake>();
		
		// Look at the target
		transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
		currentSize = SEA_SIZE;
		currentSpeed = FOLLOW_SPEED_SEA;

		// Init tile position
		UpdateTilePos();
		lastFrameTilePosX = tilePosX;
		lastFrameTilePosY = tilePosY;
		isFirstUpdate = true;
	}
	
	void UpdateTilePos()
	{
		level.WorldToTileCoords(
			transform.position.x, transform.position.y,
			out tilePosX, out tilePosY, false);
	}
	
	// Update is called once per frame
	void Update () 
	{
		//
		// Update the camera behaviour
		//
		bool shifting = false;
		float transition;
		Vector3 targetPosition = target.transform.position;

		// in water
		if(level.IsWater(targetPosition.x, targetPosition.y))
		{
			transition = SPEED_AIR_TO_SEA;
			currentSpeed = Mathf.Lerp(currentSpeed, FOLLOW_SPEED_SEA, transition);
			currentSize = Mathf.Lerp(currentSize, SEA_SIZE, SIZE_TRANSITION);
		}
		else // in air
		{
			transition = 0.01f;
			currentSpeed = Mathf.Lerp(currentSpeed, FOLLOW_SPEED_AIR, transition);
			float sizeMax = AIR_SIZE;
			// If the dragon is on painted tiles, the camera unzoom
			// Painted tiles counting
			
			int nbPaintedTiles = 0;
			int[] dxy = {-1, 0, 1};
			for(int i = 0; i < 9; i++) {
				Tile tile = level.GetTileFromWorldCoords(targetPosition.x + (dxy[i%3] * Tile.SIZE), targetPosition.y + (dxy[i/3] * Tile.SIZE));
				if(tile.IsPainted)
					nbPaintedTiles ++;
			}
			sizeMax += nbPaintedTiles * 2;
			currentSize = Mathf.Lerp(currentSize, sizeMax, SIZE_TRANSITION);
			
			if(!drakeRef.HasColor)
			{
				shifting = true;
			}
		}

		//
		// Follow the target with inertia
		// 
		
		camera.orthographicSize = currentSize;
		Vector3 pos = transform.position;
		float t = currentSpeed * Time.deltaTime;
		float x;
		float y;
		
		x = targetPosition.x; 
		y = targetPosition.y;

		 
		if(shifting)
		{
			x += airShift * target.transform.right.x;
			y += airShift * target.transform.right.y;
			//if(Vector3.Dot(drakeRef.transform.right, Vector3.right) < 0)
			//	t *= shiftTransition; //TO FIX	
		}
		pos.x = Mathf.Lerp(pos.x, x, t);
		pos.y = Mathf.Lerp(pos.y, y, t);
		
		if(shakeAmp > 0.01f)
		{
			float shakeSpeed = 15f;
			pos.x += shakeAmp * (Mathf.PerlinNoise(shakeSpeed*Time.time, 0) - 0.5f);
			pos.y += shakeAmp * (Mathf.PerlinNoise(shakeSpeed*Time.time, 100f) - 0.5f);
			shakeAmp -= 2f*Time.deltaTime;
			if(shakeAmp < 0)
				shakeAmp = 0;
		}
		
		transform.position = pos;
		
		//
		// Update tile position to wrap the level
		//
		
		UpdateTilePos();
		
		int tileOffX = tilePosX - lastFrameTilePosX;
		int tileOffY = tilePosY - lastFrameTilePosY;
		
		if(tileOffX != 0 || tileOffY != 0 || isFirstUpdate)
		{
			// Relative level wrapping
			//level.WrapFromDelta(tileOffX, tileOffY);
			
			// Absolute level wrapping (better)
			level.WrapFromCenter(tilePosX, tilePosY);
		}
		
		// TEST
		// Controls the level wrapping with arrow keys
//		if(Input.GetKeyDown(KeyCode.LeftArrow))
//			level.WrapFromDelta(-1, 0);
//		if(Input.GetKeyDown(KeyCode.RightArrow))
//			level.WrapFromDelta(1, 0);
//		if(Input.GetKeyDown(KeyCode.DownArrow))
//			level.WrapFromDelta(0, -1);
//		if(Input.GetKeyDown(KeyCode.UpArrow))
//			level.WrapFromDelta(0, 1);
//		if(Input.GetKeyDown(KeyCode.S))
//			Shake(3);
		
		lastFrameTilePosX = tilePosX;
		lastFrameTilePosY = tilePosY;
		
		isFirstUpdate = false;
	}
	
	public void Shake(float amp)
	{
		shakeAmp = amp;
	}
	
	void OnGUI()
	{
		if(Settings.debugMode)
		{
			GUI.Label(new Rect(16, 40, 256, 32), tilePosX + ", " + tilePosY, Settings.debugGuiStyle);
		}
	}

}



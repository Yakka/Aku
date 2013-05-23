using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * About coordinate systems :
 * Pixel-coords:  coordinates of the screen, the origin is on top-left and Y is down.
 * World-coords: used in the level world, Y is up.
 * Tile-coords: scaled integer version of world coords used to compute Grid coords
 * Grid-coords: unsigned integer indexes used to directly access tiles in the grid
 **/

/// <summary>
/// The level contains almost all world elements (except the player)
/// </summary>
public class Level : MonoBehaviour
{	
	private static Level globalInstance;
	
	// Generator config
	// TODO maybe put generator into an editor script for convenient level design?
	public int widthTiles = 16;
	public int heightTiles = 16;
	public int centerXTiles = 8;
	public int centerYTiles = 8;
	public int seaLevel = 1; // 0 means no sea
	public int spaceLevel = 12;
	public int levelID = 0;
	
	// Generic patterns
	public GameObject skyTilePrefab;
	public GameObject seaSurfaceTilePrefab;
	public GameObject seaTilePrefab;
	public GameObject spaceTilePrefab;
	
	// Grid of indexed tiles
	private GameObject[,] tiles;
	private int tileWrapX;
	private int tileWrapY;

	private bool finished;
	
	/// <summary>
	/// Gets the unique instance of Level.
	/// </summary>
	public static Level Get
	{
		get { return globalInstance; }
	}
	
	void Awake()
	{
		globalInstance = this;
	}
	
	void Start ()
	{
		finished = false;
		
		//
		// Get children before creating tiles
		//
		
		ArrayList nonTileChildren = new ArrayList();
		for(int i = 0; i < transform.childCount; i++)
		{
			GameObject obj = transform.GetChild(i).gameObject;
			if(obj.active)
				nonTileChildren.Add(obj);
		}
		
		//
		// Generate tiled background
		//
		
		tiles = new GameObject[widthTiles, heightTiles];

		// For each tile position
		for(int x = 0; x < widthTiles; x++)
		{
			for(int y = 0; y < heightTiles; y++)
			{
				// Determine which tile prefab to use
				GameObject chosenTilePrefab =
					y > spaceLevel ? spaceTilePrefab :
					y > seaLevel ? skyTilePrefab :
					y == seaLevel ? seaSurfaceTilePrefab :
					seaTilePrefab;
				
				if(chosenTilePrefab == null)
					continue; // No prefab !
				
				// Create a clone of the prefab
				GameObject tile = Instantiate(
					chosenTilePrefab,
					new Vector3(
						(x - centerXTiles) * Tile.SIZE,
						(y - centerYTiles) * Tile.SIZE,
						transform.position.z),
					Quaternion.Euler(0,0,0)
				) as GameObject;
				
				// Set the level as parent of the tile
				tile.transform.parent = this.transform;
				
				// Update tile position in script
				Tile tileScript = tile.GetComponent("Tile") as Tile;
				tileScript.SetGridPos(x, y);
				
				// Index the tile
				tiles[x, y] = tile;
			}
		}
		
		//
		// Link ponctual things
		//
		
		foreach(GameObject obj in nonTileChildren)
		{
			Attach(obj);
		}
	}
	
	public void Detach(GameObject obj)
	{
		Tile tileScript = GetTileFromWorldCoords(
			obj.transform.position.x,
			obj.transform.position.y);
		tileScript.Detach(obj);
	}
	
	public void Attach(GameObject obj)
	{
		Tile tileScript = GetTileFromWorldCoords(
			obj.transform.position.x,
			obj.transform.position.y);
		tileScript.Attach(obj);
	}
	
//	// Update is called once per frame
//	void Update ()
//	{
//		// NOTHINGHAHAHAHAH
//	}
	
	// Get tile corresponding to the given world position
	public Tile GetTileFromWorldCoords(float x, float y)
	{
		int tx, ty;
		WorldToTileCoords(x, y, out tx, out ty, false);
		return GetTile(tx, ty);
	}
		
	// Get tile from its position in the grid (wrapped)
	public Tile GetTile(int tx, int ty)
	{
		//Debug.Log(tx + ", " + ty + " // " + tx % widthTiles + ", " + ty % heightTiles);
		
		int txw = tx % widthTiles;
		int tyw = ty % heightTiles;		
		//Debug.Log("GetTile(" + txw + ", " + tyw + ")");
		
		// Note: in C#, modulo preserves the sign, which we don't want here.
		// However, applying an Abs() would reverse numeric order,
		// so I just shifts values to have them above zero.
		if(txw < 0)
			txw += widthTiles;
		if(tyw < 0)
			tyw += heightTiles;
		
		return tiles[txw, tyw].GetComponent("Tile") as Tile;
	}
	
	/// <summary>
	/// Converts world coordinates into world-tile coordinates.
	/// (world-tile != grid-tile coordinates !)
	/// </summary>
	/// <param name='wrap'>
	/// If true, tile coordinates will be wrapped to grid's size (make them available to use for indexing)
	/// </param>
	public void WorldToTileCoords(float x, float y, out int tx, out int ty, bool wrap)
	{
		tx = Mathf.FloorToInt(x / (float)Tile.SIZE) + centerXTiles;
		ty = Mathf.FloorToInt(y / (float)Tile.SIZE) + centerYTiles;
		if(wrap)
		{
			tx = Mathf.Abs(tx % widthTiles);
			ty = Mathf.Abs(ty % heightTiles);
		}
	}
	
	// Moves the tiles to have a wrapped level region centered around the given position
	public void WrapFromCenter (int tilePosX, int tilePosY)
	{
		int tdx = tilePosX - (widthTiles / 2) - tileWrapX;
		int tdy = tilePosY - (heightTiles / 2) - tileWrapY;
		
		//Debug.Log("Init level wrapping : " + tdx + ", " + tdy);

		for(int xi = 0; xi < Mathf.Abs(tdx); xi++)
		{
			WrapFromDelta(tdx, 0);
		}
		for(int yi = 0; yi < Mathf.Abs(tdy); yi++)
		{
			WrapFromDelta(0, tdy);
		}
	}
	
	// Puts border tiles to the opposite border for wrapping the level.
	// Note : tdx and tdy must be in [-1, 1]. If not, they will be clamped.
	public void WrapFromDelta(int tdx, int tdy)
	{
		tdx = Helper.Sign0(tdx);
		tdy = Helper.Sign0(tdy);
		//Debug.Log("WrapFromDelta(" + tdx + ", " + tdy + ")");
		
		//Debug.Log("Level wrap: " + tdx + ", " + tdy);
		if(tdx != 0)
		{
			// Wrap columns (Y)
			
			int tileOffX = tdx * widthTiles;
			int tx = tileOffX > 0 ? 0 : widthTiles-1;
			//Debug.Log("Wrap tdx="+tdx +", tileOffX="+ tileOffX);
			
			for(int ty = 0; ty < heightTiles; ty++)
			{
				GetTile(tx + tileWrapX, ty + tileWrapY).Move(tileOffX, 0);
			}
			tileWrapX += tdx;
		}
		if(tdy != 0)
		{
			// Wrap rows (X)
			
			int tileOffY = tdy * heightTiles;
			int ty = tileOffY > 0 ? 0 : heightTiles-1;
			
			for(int tx = 0; tx < widthTiles; tx++)
			{
				GetTile(tx + tileWrapX, ty + tileWrapY).Move(0, tileOffY);
			}
			tileWrapY += tdy;
		}
	}
	
	/// <summary>
	/// Tests if the given world position is under water
	/// </summary>
	public bool IsWater(float worldX, float worldY)
	{
		Tile t = GetTileFromWorldCoords(worldX, worldY);
		return t.IsWater;
	}
	
	public bool IsSpace(float worldX, float worldY)
	{
		Tile t = GetTileFromWorldCoords(worldX, worldY);
		return t.IsSpace;
	}
	
	/// <summary>
	/// Gets the height of the level in world coordinates.
	/// </summary>
	public float Height
	{
		get { return heightTiles * Tile.SIZE; }
	}
	
	/// <summary>
	/// Gets the width of the level in world coordinates.
	/// </summary>
	public float Width
	{
		get { return widthTiles * Tile.SIZE; }
	}
	
	public float GetWrappedAltitude(float worldY)
	{
		return Mathf.Repeat(
			(worldY + centerYTiles*Tile.SIZE), 
			heightTiles * Tile.SIZE
		);
	}
		
	public bool Finished
	{
		get { return finished; }
		set { finished = value; }
	}
	
}






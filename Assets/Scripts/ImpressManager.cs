using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ImpressManager
{
	//private Hashtable requests = new Hashtable();
	
	/// <summary>
	/// Requests tiles under the given object to take a render of it.
	/// This causes objects on the "Impress" layer to persist on them as a texture.
	/// The provided game object must have a mesh, because its bounds
	/// will be used to determine which tiles to call.
	/// Otherwise, this method will do nothing.
	/// Note : persistance is not done immediately, it will take effect once
	/// ImpressCamera get called by the render system of Unity.
	/// </summary>
	/// <param name='obj'>Game object that wants to persist.</param>
	public void Request(GameObject obj)
	{
		MeshFilter mf = obj.GetComponent<MeshFilter>();
		if(mf == null) {
			Debug.LogError("ImpressManager: RequestPaintImpress: no MeshFilter !");
			return;
		}
		Mesh mesh = mf.mesh;
		if(mesh == null) {
			Debug.LogError("ImpressManager: RequestPaintImpress: no Mesh !");
			return;
		}
		
		Level level = Level.Get;
		
		Bounds meshLocalBounds = mesh.bounds;
		
		Vector3 pos = obj.transform.position;
		Vector3 wmin = meshLocalBounds.min + pos;
		Vector3 wmax = meshLocalBounds.max + pos;
		
		int minX, minY, maxX, maxY;
		level.WorldToTileCoords(wmin.x, wmin.y, out minX, out minY, false);
		level.WorldToTileCoords(wmax.x, wmax.y, out maxX, out maxY, false);
		
//		Debug.Log(obj.name + ": RequestImpress "
//			+ minX + ", " + minY + ", "
//			+ maxX + ", " + maxY);
		
		Tile tile = null;
		// For each tile intersecting mesh's bounds
		for(int y = minY; y <= maxY; ++y)
		{
			for(int x = minX; x <= maxX; ++x)
			{
				tile = level.GetTile(x, y);
				tile.RequestPaintImpress(obj);
			}
		}
	}
	
	/*
	public void PreImpress(GameObject obj, Vector2i pos)
	{
		List<Vector2i> positions = null;
		if(!requests.ContainsKey(obj))
		{
			positions = new List<Vector2i>();
			requests.Add(obj, positions);
		}
		positions.Add(pos);
	}
	*/
	
	/*
	public void PostImpress(Vector2i pos)
	{
		List<Vector2i> positions = (List<Vector2i>)( requests[obj] );
		if(positions != null)
		{
			positions.Remove(pos);
			if(positions.Count == 0)
			{
				obj.SendMessage("PostImpress", SendMessageOptions.DontRequireReceiver);
			}
		}
		else
			Debug.LogError("ImpressManager: there is empty lists on PostImpress !");
	}
	*/
		
}




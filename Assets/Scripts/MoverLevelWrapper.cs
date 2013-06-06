using UnityEngine;
using System.Collections;

/// <summary>
/// This script must be attached to moving objects attached to the level,
/// so when the camera goes too far from them, they are wrapped to the other side,
/// just like tiles of the background.
/// </summary>
public class MoverLevelWrapper : MonoBehaviour 
{
	void Start () 
	{
		Check();
	}
	
	void Update () 
	{
		Check();
	}
	
	private void Check()
	{
		// Level wrapping
		
		Vector3 camPos = Camera.mainCamera.transform.position;
		Vector3 pos = transform.position;
		Vector3 prevPos = pos;
		
		float lw = Level.Get.Width;
		float lh = Level.Get.Height;
		
		float dx = camPos.x - pos.x;
		float dy = camPos.y - pos.y;
		
		if(dx >= 0.5f*lw)
		{
			pos.x += lw * (int)(Mathf.Abs(dx) / lw + 0.5f);
		}
		else if(dx <= 0.5f*lw)
		{
			pos.x -= lw * (int)(Mathf.Abs(dx) / lw + 0.5f);
		}
		if(dy >= 0.5f*lh)
		{
			pos.y += lh * (int)(Mathf.Abs(dy) / lh + 0.5f);
		}
		else if(dy <= 0.5f*lh)
		{
			pos.y -= lh * (int)(Mathf.Abs(dy) / lh + 0.5f);
		}
		
		float sd = Helper.SqrDistance(pos.x, pos.y, prevPos.x, prevPos.y);
		bool wrapped = sd > 100f;
		
//		if(wrapped)
//		{
//			Debug.Log(nnn + " " + prevPos + ", " + pos);
//			++nnn;
//		}
		
		if(wrapped)
			BroadcastMessage("PreLevelWrap", SendMessageOptions.DontRequireReceiver);
		
		transform.position = pos;
		
		if(wrapped)
			BroadcastMessage("PostLevelWrap", SendMessageOptions.DontRequireReceiver);
	}
//	static int nnn;

}





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
		float dx = camPos.x - pos.x;
		float dy = camPos.y - pos.y;
		if(Mathf.Abs(dx) >= 0.5f*Level.Get.Width)
		{
			pos.x += 2f * dx;
		}
		if(Mathf.Abs(dy) >= 0.5f*Level.Get.Height)
		{
			pos.y += 2f * dy;
		}
		transform.position = pos;
	}

}



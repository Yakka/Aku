using UnityEngine;
using System.Collections;

public class TmpGUI : MonoBehaviour 
{
	public GUIStyle style;
	
	void Start ()
	{
	
	}
	
	void Update ()
	{
	
	}

	private Rect tempRect = new Rect(200, 80, 400, 32);
	void OnGUI()
	{
		if(Level.Get.Finished)
		{
			GUI.Label(tempRect, "Fin du niveau", style);
		}
	}

}

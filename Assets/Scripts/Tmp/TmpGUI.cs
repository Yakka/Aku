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

	private Rect tempRect = new Rect(300, 50, 400, 32);
	private float timer;
	private bool timing = false;
	void OnGUI()
	{
		if(Level.Get.Finished)
		{
			if(!timing)
			{
				timing = true;
				timer = 3f + Time.time;
			}
			if(timer < Time.time)
			{
				GUI.Label(tempRect, "Touchez pour revenir au menu", style);
			}
		}
	}

}

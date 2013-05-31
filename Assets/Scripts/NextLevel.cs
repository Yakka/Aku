using UnityEngine;
using System.Collections;

public class NextLevel : MonoBehaviour {
	
	public string nextLevelName = "";
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other.name == "Drake")
		{
			if(nextLevelName != "")
				Application.LoadLevel (nextLevelName);
		}
	}
}

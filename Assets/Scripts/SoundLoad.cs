using UnityEngine;
using System.Collections;

public class SoundLoad : MonoBehaviour {
	
	public string nextScene;
	
	// Use this for initialization
	void Start () {
	
		//Chargement michele
		
		
		//---
		
		Application.LoadLevel(nextScene);
	}
}

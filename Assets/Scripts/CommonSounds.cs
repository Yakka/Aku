using UnityEngine;
using System.Collections;

public class CommonSounds : MonoBehaviour {
	private static CommonSounds globalInstance;
	
	void Awake()
	{
		globalInstance = this;
	}
	
	public static CommonSounds Instance
	{
		get { return globalInstance; }
	}
	
	void Start () {
	
	}
	
	void Update () {
		
	}
	
	public void MoonEnter() {
		
	}
	
	public void MoonExit() {
		
	}
	
	public void playSplash() { // taille du dragon?
		
	}
	
}

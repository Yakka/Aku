using UnityEngine;
using System.Collections;

public class SoundLevel2 : MonoBehaviour {
	private static SoundLevel2 globalInstance;
	
	
	void Awake()
	{
		globalInstance = this;
	}
	
	public static SoundLevel2 Instance
	{
		get { return globalInstance; }
	}
	
	void Start () {
		
	}
	
	void Update () {

	}
	
	public void HitStar(GameObject star) {
		
	}
	
	public void SwapStar() {
		
	}
	
	public void HitPetrol() {
		
	}
	
	public void SizePetrol(int size) {
		
	}
	
	public void CloudEnter() { //int emotion pas encore
		
		
	}
	
	public void CloudExit() {
		
	}
	
}

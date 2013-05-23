using UnityEngine;
using System.Collections;

public class SoundLevel1 : MonoBehaviour {
	private static SoundLevel1 globalInstance;
	
	void Awake()
	{
		globalInstance = this;
	}
	
	public static SoundLevel1 Instance
	{
		get { return globalInstance; }
	}

	void Start () {
		
	}
	
	void Update () {
		
	}
	
	
	public void HitStar(GameObject star) {
		
	}
	
	public void CloudExit(GameObject cloud) {
		
	}
	
	private /*IEnumerator*/void TimerIntro() {
		
	}
	
	public void HornetOnHead() {
		
	}
	
	public void DrakeFree() {
		
	}
	
	public void HornetBirth() {
		
	}
	
	public void SizeHornet(int size) {
		
	}
	
	public void LadybirdMoving() {
		
	}
	
	public void LadybirdStoping() {
		
	}
	
	public void EnlargeDrake() {
		
	}
	
}

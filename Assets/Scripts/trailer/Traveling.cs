using UnityEngine;
using System.Collections;

public class Traveling : MonoBehaviour {
	
	public Transform target;
	public float speed = 0.1f;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		transform.position = Vector3.Lerp(transform.position, target.position, speed);
		
	}
}

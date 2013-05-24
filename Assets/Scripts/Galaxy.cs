using UnityEngine;
using System.Collections;

public class Galaxy : MonoBehaviour {
	
	private Vector3 pivot;
	private float angularSpeed = 1f;
	private float speed = 2f;
	
	// Use this for initialization
	void Start () {
		pivot = transform.position + Vector3.right * 100;
	}
	
	// Update is called once per frame
	void Update () {
		transform.RotateAround(pivot, Vector3.forward, angularSpeed * Time.deltaTime);
		transform.position += Vector3.up * speed * Time.deltaTime;
	}
}

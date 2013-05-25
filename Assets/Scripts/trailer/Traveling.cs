using UnityEngine;
using System.Collections;

public class Traveling : MonoBehaviour {
	
	public CameraTrigger[] triggers;
	private int index = 0;
	
	private float currentSize;
	private float currentSpeed;
	private float pauseTime;
	
	Vector3 direction;
	bool paused = false;
	
	// Use this for initialization
	void Start () {
		pauseTime = Time.time;
		transform.position = triggers[index].transform.position;
		currentSize = triggers[index].size;
		currentSpeed = triggers[index].speed;
		direction = new Vector3(0,0,0);
	}
	
	// Update is called once per frame
	void Update () {
	
		Vector3 targetPosition;
		if(Vector3.Distance(transform.position, triggers[index].transform.position) < 15f && index < triggers.Length - 1)
		{
			if(!paused)
			{
				pauseTime = Time.time + triggers[index].pause;
				paused = true;
			}
			
		}
		if(paused)
		{
			if(Time.time >= pauseTime)
			{
				index ++;
				paused = false;
				
			}
			else 
				currentSpeed = Mathf.Lerp(currentSpeed, 0f, 0.1f);
		}
		else
		{
			currentSpeed = Mathf.Lerp(currentSpeed, triggers[index].speed, 0.005f);
			currentSize = Mathf.Lerp(currentSize, triggers[index].size, 0.001f);
		}
		targetPosition = triggers[index].transform.position;
		//float x;
		//float y;
		//x = triggers[index].transform.position.x; 
		//y = triggers[index].transform.position.y;
		//transform.position = Vector3.Lerp(transform.position, targetPosition, currentSpeed);
		

		// Find orientation vector to the target
		Vector3 vectorToTarget = targetPosition - transform.position; // No movement by default

		vectorToTarget.Normalize();
		
		// Compute current orientation as smoothed from target orientation
		direction = Vector3.Lerp(direction, vectorToTarget, 0.1f);
		camera.orthographicSize = currentSize;
		transform.position += direction * Time.deltaTime * currentSpeed;
	}
}

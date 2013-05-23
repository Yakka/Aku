using UnityEngine;
using System.Collections;

/// <summary>
/// Companion bug
/// </summary>
public class Ladybird : MonoBehaviour 
{
	public Transform[] targets;
	public LadybirdTrigger[] initialPositions;
	public Transform drake;
	
	private int state;
	private const int STATE_MOVING = 0;
	private const int STATE_PAUSE = 1;
	private const int STATE_WAITING = 2;
	private const int STATE_SLEEPING = 3;
	private const int STATE_SEARCHING = 4; // The bug is searching for the drake
	private const int MAX_WAITING = 15; // Time in seconds of waiting if the player doesn't follow the ladybird
	private float nextSearch;
	
	private const float ANGULAR_LERP_COEF = 0.1f; // Must be between 0 and 1 
	private int index = 0; // Between nextIndex and nextIndex-1 
	public const float SPEED = 30f; // Bug speed is constant. Because it's a bug.
	private bool triggered = false;

	// Use this for initialization
	void Start ()
	{
		if(targets == null)
			Debug.LogError("Ladybird needs targets!");
		else
		{
			if(initialPositions == null)
				Debug.LogError("Ladybird needs initialPositions!");
			else
			{
				transform.position = initialPositions[0].transform.position;
				state = STATE_SLEEPING;
				renderer.enabled = false;
			}
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(targets == null || initialPositions == null)
			return;
		
		switch(state)
		{
		case STATE_SLEEPING:
			for(int i = 0; i < initialPositions.Length; i++)
			{
				if(initialPositions[i].IsActivated())
				{
					state = STATE_WAITING;
					transform.position = initialPositions[i].transform.position;
					renderer.enabled = true;
				}
			}
			break;
		case STATE_MOVING:
			
			Vector3 target = targets[index].position;
			float distanceToTarget = Vector2.Distance(target, transform.position);
			
			// If I'm not on my current target
			if(distanceToTarget > 0.1f)
			{
				MoveTo (target);
				
			}
			else
			{
				state = STATE_WAITING;
				nextSearch = Time.time + MAX_WAITING;
				SoundLevel1.Instance.LadybirdStoping();
				animation.Play("land");
				
			}
		
		break;
		case STATE_WAITING:
			if(triggered)
				Disturb();
			else if(Time.time > nextSearch)
			{
				state = STATE_SEARCHING;
				animation.Play ("takeOff");
				SoundLevel1.Instance.LadybirdMoving();
			}
			else if(!animation.isPlaying)
			{
				if(Random.Range(0f, 1f) < 0.05f)
					animation.Play ("stand");
			}
			
		break;
		case STATE_SEARCHING:
			Vector3 drakePosition = drake.position;
			float distanceToDrake = Vector2.Distance(drakePosition, transform.position);
			// If I'm not on my current target
			if(distanceToDrake > 20f)
			{
				MoveTo (drakePosition);
			}
			else
			{
				state = STATE_MOVING;
			}
			break;
		}
	}
	
	public void MoveTo(Vector3 target)
	{
		float distanceToTarget = Vector2.Distance(target, transform.position);
		if(distanceToTarget <= SPEED * Time.deltaTime)
		{
			transform.position = new Vector3(target.x, target.y, 0);
		}
		else
		{
			Vector3 direction3 = target - transform.position;
			direction3.z = 0;
	        Vector2 direction2 = new Vector2(direction3.x, direction3.y);
			direction2.Normalize();
			direction3.Normalize();
			
			// FIXME hardcode poo
			float targetAngle = 
				Mathf.Atan2(direction2.y, direction2.x) * Mathf.Rad2Deg 
				+ Mathf.PerlinNoise(transform.position.x, transform.position.y) * 10f - 90f;

			transform.rotation = 
				Quaternion.Lerp(
					transform.rotation, 
					Quaternion.Euler (0f, 0f, targetAngle), 
					ANGULAR_LERP_COEF);
			
			transform.position += direction3 * SPEED * Time.deltaTime;
		}
		
		if(!animation.isPlaying) // Play the animation
		{
			
			animation.Play("fly");
		}
		
	}
	
	public void NextTarget()
	{
		if(index < targets.Length - 1)
		{
			index ++;
		}
	}
	
	public void Disturb()
	{
		if(state == STATE_WAITING)
		{
			if(index < targets.Length - 1)
				index ++;
			else
				index = 0;
			animation.Play("takeOff");
			state = STATE_MOVING;
			SoundLevel1.Instance.LadybirdMoving();
		}
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other.name == "Drake")
		{
			triggered = true;
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		if(other.name == "Drake")
		{
			triggered = false;
		}
	}

}

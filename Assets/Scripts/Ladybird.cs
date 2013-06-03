using UnityEngine;
using System.Collections;

/// <summary>
/// Companion bug
/// </summary>
public class Ladybird : MonoBehaviour 
{
	public LadybirdTrigger[] targets;
	public LadybirdTrigger[] initialPositions;
	public Transform drake;
	
	//Exception code for the faces-level
	public SwappableStar[] stars;
	
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
	private float pauseTrailer;
	
	protected Quaternion originalRotation;

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
		if(Settings.trailerMode)
		{
			state = STATE_WAITING;
			Disturb ();
		}
		
		originalRotation = transform.rotation;
		//index = 15;
		
		//DEBUG
		//index = 10;
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
			
			Vector3 target = targets[index].transform.position;
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
				pauseTrailer = Time.time + 1;
				if(!Settings.trailerMode)
					CommonSounds.Instance.LadybirdStoping();
				animation.Play("land");
			}
		
		break;
		case STATE_WAITING:
			if(Time.time > pauseTrailer && Settings.trailerMode)
			{
				animation.Play ("takeOff");
				Disturb();
				if(!Settings.trailerMode)
					CommonSounds.Instance.LadybirdMoving();
			}
			else if(triggered)
			{
				Disturb();
			}
			else if(Time.time > nextSearch && !Settings.trailerMode)
			{
				state = STATE_SEARCHING;
				animation.Play ("takeOff");
				if(!Settings.trailerMode)
					CommonSounds.Instance.LadybirdMoving();
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
			transform.position = new Vector3(target.x, target.y, transform.position.z);
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
					Quaternion.Euler (0f, 0f, targetAngle) * originalRotation, 
					ANGULAR_LERP_COEF);
			
			transform.position += direction3 * SPEED * Time.deltaTime;
		}
		
		if(!animation.isPlaying) // Play the animation
		{
			
			animation.Play("fly");
		}
		
	}
	

	
	public void Disturb()
	{
		if(state == STATE_WAITING)
		{
			bool next = false;
			bool exception = false;
			
			// Check if we need to wait a star at the good place
			if(Level.Get.levelID == 2)
			{
				if(targets[index].name == "LadyBirdTriggerBlueStar" ||
					targets[index].name == "LadyBirdTriggerRedStar" ||
					targets[index].name == "LadyBirdTriggerYellowStar")
				{
					int nextIndex = 0;
					string nextName = "";
					exception = true;
					foreach(SwappableStar star in stars)
					{
						if(star.IsAtGoodPosition())
						{
							switch(targets[index].name)
							{
							case "LadyBirdTriggerBlueStar":
								nextName = "LadyBirdTriggerOldmanGood";
								break;
							case "LadyBirdTriggerRedStar":
								nextName = "LadybirdTriggerWomanGood";
								break;
							case "LadyBirdTriggerYellowStar":
								nextName = "LadyBirdTriggerBabyGood";
								break;
							}
							//TOFIX
							renderer.enabled = false;
							next = true;
							for(int i = 0; i < targets.Length; i++)
							{
								if(targets[i].name == nextName)
								{
									//index = i; TOFIX
									//index = 16;
									Debug.Log("Next Index : "+i);
								}
							}
							break;
						}
					}
					if(next)
					{
						index = nextIndex;
					}
					else
					{
						for(int i = 0; i < targets.Length; i++)
						{
							if(targets[i].name == "LadyBirdTriggerMoon")
							{
								index = i;
							}
						}
					}
				}
			}
			
			// Get to a random star !
			if(Level.Get.levelID == 2)
			{
				if(!exception && targets[index].name == "LadyBirdTriggerMoon")
				{
					exception = true;
					index += Random.Range(1, 3);
					if(index > targets.Length - 1)
					{
						Debug.Log ("Error, too small array! Or maybe the index is to big.");
						index = 0;
					}
				}
			}
			
			// Check if we need to wait a hidden painting
			if(!exception)
			{
				if(!targets[index].IsHiddenPaintingTrigger())
				{
					next = true;
				}
				if(next)
				{
					if(index < targets.Length - 1)
					{
						index ++;
						if(index == 2)
							transform.position = new Vector3 (transform.position.x, transform.position.y, -11);
					}
					else
						if(Level.Get.levelID != 2)
							index = 0;
				}
				else
				{
					if(index > 0)
					{
						index --;
					}
					else 
						index = targets.Length - 1;
				}
				
			}
			// TOFIX DIRTY CODE
			if(index == 16)
				renderer.enabled = false;
			animation.Play("takeOff");
			state = STATE_MOVING;
			if(!Settings.trailerMode)
				CommonSounds.Instance.LadybirdMoving();
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

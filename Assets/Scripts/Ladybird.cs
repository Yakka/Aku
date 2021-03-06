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
	public Transform prison;
	
	private HaloBehaviour halo;
	private float haloTimer;
	private const float HALO_TIME = 1f;
	private bool isHaloing = false;
	
	//Exception code for the faces-level
	public SwappableStar[] stars;
	
	private int state;
	private const int STATE_MOVING = 0;
	private const int STATE_PAUSE = 1;
	private const int STATE_WAITING = 2;
	private const int STATE_SLEEPING = 3;
	private const int STATE_SEARCHING = 4; // The bug is searching for the drake
	private const int STATE_JOY = 5; //The player did a good thing
	private const int STATE_GRABBED = 6; //The player did a good thing
	private const int MAX_WAITING = 15; // Time in seconds of waiting if the player doesn't follow the ladybird
	private float nextSearch;
	
	private const float ANGULAR_LERP_COEF = 0.1f; // Must be between 0 and 1 
	private int index = 0; // Between nextIndex and nextIndex-1 
	private const float SPEED = 30f; // Bug speed is constant. Because it's a bug.
	private bool triggered = false;
	
	private Quaternion originalRotation;

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
		int nbChild = transform.GetChildCount();
		for(int i = 0; i < nbChild; i++)
		{
			Transform child = transform.GetChild(i);
			if(child != null)
			{
				halo = child.GetComponent<HaloBehaviour>();
				if(halo != null)
					break;
			}
		}
		
		originalRotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(targets == null || initialPositions == null)
			return;
		
		if(halo != null && Time.time >= haloTimer && isHaloing)
		{
			isHaloing = false;
			halo.SetON(false);
		}
		
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
				CommonSounds.Instance.LadybirdStoping();
				animation.Play("land");
			}
		
		break;
		case STATE_WAITING:
			if(triggered)
			{
				nextSearch = Time.time + MAX_WAITING;
				Disturb();
			}

			else if(Time.time > nextSearch)
			{
				state = STATE_SEARCHING;
				animation.Play ("takeOff");
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
				CommonSounds.Instance.LadybirdCall();
				if(halo != null)
				{
					halo.SetON(true);
					haloTimer = Time.time + HALO_TIME;	
					isHaloing = true;
				}
				state = STATE_MOVING;
			}
			break;
		case STATE_JOY:
			transform.rotation = 
				Quaternion.Lerp(
					transform.rotation, 
					Quaternion.Euler (0f, 0f, 360) * originalRotation, 
					ANGULAR_LERP_COEF);
			if(!animation.isPlaying) // Play the animation
			{	
				animation.Play("fly");
			}
			break;
		case STATE_GRABBED:
			float distanceToPrison = Vector2.Distance(prison.position, transform.position);
			
			// If I'm not on my current target
			if(distanceToPrison > 0.1f)
				MoveTo(prison.position);
			else
				Helper.SetActive(gameObject, false);
			break;
		}
		
		
	}
	
	public void GrabbedByHornet()
	{
		state = STATE_GRABBED;
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
			bool move = false;
			bool annoyed = false;
			
			// Check if we need to wait a star at the good place
			switch(targets[index].name)
			{
			//
			// The ladybird stays on the face until it's painted
			//
			case "LadyBirdTriggerBabyFace1":
			case "LadyBirdTriggerWomanFace1":
			case "LadyBirdTriggerOldmanFace1":
				transform.position = new Vector3
					(transform.position.x,
					transform.position.y,
					-11);
				if(!targets[index].IsHiddenPaintingTrigger())
				{
					move = true;
					index ++;
				}
				else 
					annoyed = true;
				break;
			//
			// The ladybird stays on the face until it's painted 
			// 
			case "LadyBirdTriggerFaceBaby2":
			case "LadybirdTriggerFaceWoman2":
			case "LadyBirdTriggerFaceOldman2":
				annoyed = true;
				break;
			//
			// The ladybird is kidnapeed by hornets
			//
			case "LadyBirdTriggerPetrol":
				break;
			//
			// The ladybird comes back to the moon if stars are not at the right position
			// If a star is at the right position, the ladybird goes to the hidden face which is matching
			//
			case "LadyBirdTriggerBlueStar":
			case "LadyBirdTriggerRedStar":
			case "LadyBirdTriggerYellowStar":
				bool wrongPosition = true;
				string nextName = "";
				foreach(SwappableStar star in stars)
				{
					if(star.IsAtGoodPosition() && wrongPosition)
					{
						wrongPosition = false;
						switch(star.positionID)
						{
						case 0:
							nextName = "LadybirdTriggerFaceWoman2";
							break;
						case 1:
							nextName = "LadyBirdTriggerFaceOldman2";
							break;
						case 2:
							nextName = "LadyBirdTriggerFaceBaby2";
							break;
						}
						for(int i = 0; i < targets.Length; i++)
						{
							if(targets[i].name.Equals(nextName))
							{
								index = i;
								move = true;
								break;
							}
						}
						break;
					}
				}
				if(wrongPosition)
				{
					for(int i = 0; i < targets.Length; i++)
					{
						if(targets[i].name == "LadyBirdTriggerMoon")
						{
							index = i;
							move = true;
							break;
						}
					}
				}
				break;
			//
			// The ladybird goes to a random star
			//
			case "LadyBirdTriggerMoon":
				int[] indexStars = new int[3];
				
				for(int i = 0; i < targets.Length; i++)
				{
					if(targets[i].name == "LadyBirdTriggerBlueStar")
						indexStars[0] = i;
					if(targets[i].name == "LadyBirdTriggerRedStar")
						indexStars[1] = i;
					if(targets[i].name == "LadyBirdTriggerYellowStar")
						indexStars[2] = i;
				}
				index = indexStars[Random.Range(0, indexStars.Length)];
				move = true;
				break;
			default:
				move = true;
				index ++;
				break;
			}
			
			if(move)
			{
				animation.Play("takeOff");
				state = STATE_MOVING;
				CommonSounds.Instance.LadybirdMoving();
			} else if(annoyed && !animation.isPlaying)
			{
				animation.PlayQueued("takeOff", QueueMode.CompleteOthers);
				animation.PlayQueued("land", QueueMode.CompleteOthers);
				
				// MICHELE son de "coccinelle attend que tu fasses quelque chose"
				CommonSounds.Instance.LadybirdShake();
			}
			
			if(index > targets.Length - 1)
			{
				index = 0;
				Debug.Log ("Error, end of triggers");
			}
			
			// TOFIX DIRTY CODE
			if(index == 13)
				renderer.enabled = false;
			
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

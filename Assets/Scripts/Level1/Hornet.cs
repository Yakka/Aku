using UnityEngine;
using System.Collections;

public class Hornet : MonoBehaviour 
{	
	public Transform target;
	public float SPEED_MAX = 50f;
	public float REACTIVITY = 1f; // The lower it is, the slower hornets will change their orientation.
	public float VIEW_DISTANCE = 200;
	private const int LIFE_MAX = 3;
	private const float FALL_GRAVITY = 100f;
	
	//private float speed;
	private Vector3 orientation; // Unit-inferior vector
	private ArrayList crossedClouds; // Which paint clouds hornets have crossed yet
	private int life; // How many times hornets must be hurt before die
	private bool grabbingTarget = false; // True if hornets are grabbing their target
	private bool targetIsInSea = false; // true when hornets fly out of sea
	private bool isInCloud = false;
	public Vector3 fallVelocity; // Only used when falling dead
	private float initialScale;
	private bool willRelease = false; // If true, hornet release target after a while
	
	// If they are grabbing a target, hornets must release it after this timestamp 
	private float releaseTargetTime;
	// Hornets can chase a target after this timestamp
	private float chaseStartTime; 
	
	// Use this for initialization
	void Start ()
	{
		orientation = transform.right;
		crossedClouds = new ArrayList();
		life = LIFE_MAX;
		initialScale = transform.localScale.x;
	}
	
	private bool CanSeeTarget(float targetX, float targetY)
	{
		return !Level.Get.IsWater(targetY, targetY) 
			&& !Level.Get.IsSpace(targetX, targetY)
			&& Helper.SqrDistance(
					targetX, targetY, 
					transform.position.x, 
					transform.position.y) < VIEW_DISTANCE*VIEW_DISTANCE;
	}
	
	private bool CanChase()
	{
		return Time.time >= chaseStartTime && life > 0;
	}
	
	/// <summary>
	/// Starts grabbing the target and sets grabbingTarget to true.
	/// Does nothing if it already grabs a target.
	/// It may causes trouble to the target.
	/// It also sets a release time after which hornets will release their target.
	/// </summary>
	private void GrabTarget()
	{
		if(!grabbingTarget)
		{
			SoundLevel1.Instance.HornetOnHead();
			Drake drake = target.GetComponent<Drake>();
			if(drake != null)
			{
				drake.GrabbedByHornet();
				willRelease = true;
			}
			Ladybird ladybird = target.GetComponent<Ladybird>();
			if(ladybird != null)
			{
				ladybird.GrabbedByHornet();
				willRelease = false;
			}
			grabbingTarget = true;
			releaseTargetTime = Time.time + Random.Range(4f, 8f);
		}
	}
	
	/// <summary>
	/// Releases the target and briefly disables chase phase.
	/// Does nothing if the target is not grabbed.
	/// </summary>
	private void ReleaseTarget()
	{
		if(grabbingTarget)
		{
			SoundLevel1.Instance.DrakeFree();
			Drake drake = target.GetComponent<Drake>();
			if(drake != null)
				drake.UngrabbedByHornet();
			grabbingTarget = false;
			chaseStartTime = Time.time + Random.Range(0.5f, 1.5f);
				
		}
	}

	void Update ()
	{
		if(life <= 0)
		{
			Vector3 pos = transform.position;
			
			if(!isInCloud)
			{
				if(fallVelocity.y > -50f)
				{
					fallVelocity.y -= FALL_GRAVITY * Time.deltaTime;
				}
				
				if(fallVelocity.x > 0.01f)
				{
					fallVelocity.x -= 10f*Time.deltaTime;
					if(fallVelocity.x < 0)
						fallVelocity.x = 0;
				}
				else if(fallVelocity.x < -0.01f)
				{
					fallVelocity.x += 10f*Time.deltaTime;
					if(fallVelocity.x > 0)
						fallVelocity.x = 0;
				}
			}
			
			pos.x += fallVelocity.x * Time.deltaTime;
			pos.y += fallVelocity.y * Time.deltaTime;
				
			transform.position = pos;
			
			if(Level.Get.IsWater(pos.x, pos.y))
			{
				// Disappear once in water
				gameObject.active = false;
			}
		}
		else
		{
			// Debug
			if(!Settings.onTablet)
			{
				if(Input.GetKeyDown(KeyCode.K))
				{
					Hurt();
				}
			}
			
			if(grabbingTarget)
			{
				transform.position = target.position;
			}
			else // Find orientation
			{
				// 2D Conversion
				Vector2 position2D = new Vector2(transform.position.x, transform.position.y);
				Vector2 target2D = new Vector2(target.position.x, target.position.y);
	
				// Find orientation vector to the target
				Vector2 vectorToTarget = new Vector2(0,0); // No movement by default
				if(targetIsInSea || Level.Get.IsWater(position2D.x, position2D.y))
				{
					// Go up
					vectorToTarget.x = 0;
					vectorToTarget.y = 1;
				}
				else if(CanChase() && CanSeeTarget(target2D.x, target2D.y))
				{
					// Approach the target
					vectorToTarget.x = target2D.x - position2D.x;
					vectorToTarget.y = target2D.y - position2D.y;
					float m = vectorToTarget.magnitude;
					vectorToTarget.x /= m;
					vectorToTarget.y /= m;
				}
				// Compute current orientation as smoothed from target orientation
				float t = REACTIVITY * Time.deltaTime;
				orientation.x = Mathf.Lerp(orientation.x, vectorToTarget.x, t);
				orientation.y = Mathf.Lerp(orientation.y, vectorToTarget.y, t);	
				
				transform.position += orientation * Time.deltaTime * SPEED_MAX;
			}
			
			// Release target if it goes underwater
			if(Level.Get.IsWater(transform.position.x, transform.position.y)) 
			{
				if(grabbingTarget) 
				{
					ReleaseTarget();
				}
				targetIsInSea = true;
			}
			else
				targetIsInSea = false;
			
			// Release target if chase has been disabled
			if(grabbingTarget && Time.time >= releaseTargetTime && willRelease)
			{
				ReleaseTarget();
			}
		}
	}
	
	void OnTriggerEnter (Collider other)
	{
		Cloud cloud = other.GetComponent<Cloud>();
		
		if (cloud != null)
			OnCloudEnter (cloud);
		else if(other.transform == target) 
		{
			if(life > 0)
				GrabTarget();
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		Cloud cloud = other.GetComponent<Cloud>();
		if(cloud != null)
		{
			OnCloudExit(cloud);
		}
	}
	
	void OnCloudEnter (Cloud cloud)
	{
		isInCloud = true;
		if(!crossedClouds.Contains(cloud)) 
		{
			Vector3 pos = transform.position;
			float s = transform.localScale.x / 2f;
			for(int i = 0; i < 3; ++i)
			{
				if(Level.Get.levelID == 2)
				{
					PolyPaintManager.Instance.SpawnDrip(cloud.colorIndex, 
						Random.Range(pos.x - s, pos.x + s),
						Random.Range(pos.y - s, pos.y + s));
				}
				else
				{
					PolyPaintManager.Instance.SpawnDrip(cloud.CurrentColor, 
						Random.Range(pos.x - s, pos.x + s),
						Random.Range(pos.y - s, pos.y + s));
				}
			}
			Hurt();
			crossedClouds.Add(cloud);
			SoundLevel1.Instance.SizeHornet(life);
		}
	}
	
	void Hurt()
	{
		if(life <= 0)
			return;
		
		--life;
		
		// TODO Hornet: remove this when the particle rendering will be done
		float s = initialScale * (0.5f + 0.5f * (float)life / (float)LIFE_MAX);
		transform.localScale = new Vector3(s,s,1);
		
		Debug.Log("Hornet life : " + life);
		if(life <= 0) 
		{
			//gameObject.active = false;
			Debug.Log("Hornet fainted");
			if(grabbingTarget)
			{
				ReleaseTarget();
			}
			OnDeath();
			fallVelocity = new Vector3(
				orientation.x * SPEED_MAX,
				orientation.y * SPEED_MAX, 0);
		}
	}
	
	void OnCloudExit(Cloud cloud)
	{
		isInCloud = false;
	}
	
	void OnDeath()
	{
		// DEATH OF THE HORNET
	}
	
	public bool GrabbingTarget
	{
		get { return grabbingTarget; }
	}

}




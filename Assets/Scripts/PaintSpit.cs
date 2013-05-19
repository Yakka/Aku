using UnityEngine;
using System.Collections;

public class PaintSpit : MonoBehaviour 
{
	const float GRAVITY = 0.3f; // In world units / s^2
	//const float DISTANCE_AUTONOMY = 400; // In world units
	
	private Vector3 vel;
	private Vector3 pos;
	private PainterBehavior ppb;
	private sbyte channel = -1; // Optionnal parameter for level 2
	
	void Start ()
	{
		ppb = GetComponent<PainterBehavior>();
		
		// Scale variations
		float r = Random.Range(0.6f, 1.4f);
		Vector3 scale = transform.localScale;
		scale.x *= r;
		scale.y *= r;
		transform.localScale = scale;
	}
	
	void Update ()
	{
		float dt = Time.deltaTime;
		vel.y -= GRAVITY * dt;
		pos.x += vel.x * dt;
		pos.y += vel.y * dt;
		transform.position = pos;
		
		if(ppb.ColorLevel < 0.001f || Level.Get.IsWater(pos.x, pos.y))
		{
			//Debug.Log("PaintSpit ended : " + ppb.ColorLevel + ", " + Level.Get.IsWater(pos.x, pos.y));
			gameObject.active = false;
		}
	}
	
	public void Reset(Vector3 newPos, Vector3 newVel, Color newColor)
	{
		//Debug.Log("Activate PaintSpit");
		if(ppb == null)
		{
			ppb = GetComponent<PainterBehavior>();
		}
		
		ppb.SetColor(newColor, true);
		ppb.ColorLevel = 1;
		//ppb.distanceAutonomy = DISTANCE_AUTONOMY;
		vel = newVel;
		pos = newPos;
		transform.position = pos;
		
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other.name == "PetrolBall")
		{
			gameObject.active = false;
		}
	}
	
	/// <summary>
	/// Specific to the 2nd level of Aku.
	/// Gets or sets the associated channel of the paint, in order to 
	/// know to which character the color is associated to.
	/// Its value is in [-1, 2], where -1 means no channel
	/// </summary>
	/// <value>The channel index in [-1, 2], where -1 means no channel</value>
	public sbyte Channel
	{
		get { return channel; }
		set { channel = (sbyte)Mathf.Clamp(value, -1, 2); }
	}

}


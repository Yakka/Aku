using UnityEngine;
using System.Collections;

public class PaintSpit : MonoBehaviour 
{
	const float GRAVITY = 0.3f; // In world units / s^2
	//const float DISTANCE_AUTONOMY = 400; // In world units
	
	private Vector3 vel;
	private Vector3 pos;
	private PainterBehavior pb;
	
	void Start ()
	{
		pb = GetComponent<PainterBehavior>();
		if(pb == null)
		{
			Debug.LogError(name + ": I need PainterBehavior.cs !");
		}
		pb.affectedByClouds = false;
		
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
		
		if(pb.ColorLevel < 0.001f || Level.Get.IsWater(pos.x, pos.y))
		{
			//Debug.Log("PaintSpit ended : " + ppb.ColorLevel + ", " + Level.Get.IsWater(pos.x, pos.y));
			Helper.SetActive(gameObject, false);
		}
	}
	
	public void Reset(Vector3 newPos, Vector3 newVel, sbyte channel)
	{
		//Debug.Log("Activate PaintSpit (channel=" + channel + ")");
		if(pb == null)
		{
			pb = GetComponent<PainterBehavior>();
		}
		
		vel = newVel;
		pos = newPos;
		transform.position = pos;
		
		// Note : recharge after setting position because otherwise the 
		// distance autonomy may be dramatically affected on next frame
		// (See pb.ColorLevel implementation).
		pb.SetColor(channel, true);
		pb.ColorLevel = 1; 
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other.name == "PetrolBall")
		{
			Helper.SetActive(gameObject, false);
		}
	}
	
	/// <summary>
	/// Specific to the 2nd level of Aku.
	/// Gets the associated channel of the paint, in order to 
	/// know to which character the color is associated to.
	/// Its value is in [-1, 2], where -1 means no channel
	/// </summary>
	/// <value>The channel index in [-1, 2], where -1 means no channel</value>
	public int Channel
	{
		get { return pb.ColorIndex; }
		//set { channel = (sbyte)Mathf.Clamp(value, -1, 2); }
	}

}


using UnityEngine;
using System.Collections;

public class PetrolBall : MonoBehaviour 
{
	const float HIT_TIME_DELAY = 5f;
	
	private float[] lastHitTime; // Last time a PaintSpit hit the PetrolBall for each different color
	private float normalScale;
	
	// Use this for initialization
	void Start () 
	{
		lastHitTime = new float[3];
		for(uint i = 0; i < lastHitTime.Length; ++i)
			lastHitTime[i] = -9999; // Init to a time far far away
		
		normalScale = transform.localScale.x;
	}
	
	// Update is called once per frame
	Vector3 tempScale;
	void Update ()
	{
		float t = Time.time;
		uint i = 0;
		for(; i < lastHitTime.Length; ++i)
		{
			if(t - lastHitTime[i] > HIT_TIME_DELAY)
				break; // One of the hits dates from too long. Stopping.
		}
		if(i == lastHitTime.Length) // Entire loop done?
		{
			// Each of the hits have occurred in the last HIT_TIME_DELAY seconds.
			// The PetrolBall must die.
			Debug.Log("The PetrolBall died :D");
			gameObject.active = false;
			// TODO PetrolBall : fancier explode animation
			Level.Get.Finished = true;
		}
		
		// Some cheap funky animation :
		// Roooootation
		transform.Rotate(0, 0, 1000f*Time.deltaTime);
		// Cooooontraction
		tempScale.x = normalScale * (0.95f + 0.1f*Mathf.PerlinNoise(5f*Time.time, 10));
		tempScale.y = tempScale.x;
		transform.localScale = tempScale;
	}
	
	void OnTriggerEnter(Collider other)
	{
		PaintSpit ps = other.GetComponent<PaintSpit>();
		if(ps != null)
		{
			if(ps.Channel != -1)
			{
				lastHitTime[ps.Channel] = Time.time;
				SoundLevel2.Instance.SizePetrol(0);
			}
		}
	}

}



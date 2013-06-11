using UnityEngine;
using System.Collections;

public class HaloBehaviour : MonoBehaviour 
{
	private const float ANIM_DURATION = 1f; // In seconds
	
	private float initialScale;
	private bool on = false;
	private float commuteTime;
	
	void Start () 
	{
		initialScale = transform.localScale.x;
		commuteTime = Time.time;
		//on = false;
		if(HaloManager.Instance == null)
		{
			Debug.LogError(name + ": I need HaloManager ! "
				+ "Just add an empty game object to the scene "
				+ "with HaloManager script on it.");
		}
	}
	
	public bool On
	{
		get { return on; }
	}
	
	public void SetON(bool v)
	{
		//Debug.Log("Commute " + on + " -> " + v);
		if(on ^ v)
		{
			commuteTime = Time.time;
			on = v;
		}
//		if(on)
//		{
//			gameObject.active = true;
//		}
	}
	
	void Update ()
	{
		float delta = Time.time - commuteTime;
	
		AnimationCurve curve = null;
		if(on)
		{
			if(delta < ANIM_DURATION)
				curve = HaloManager.Instance.startCurve;
			else
				curve = HaloManager.Instance.idleCurve;
		}
		else
		{
			if(delta < ANIM_DURATION)
				curve = HaloManager.Instance.endCurve;
//			else
//				gameObject.active = false;
		}
		
		if(curve != null)
		{
			float t = Mathf.Repeat(delta/ANIM_DURATION, 1f);
			Vector3 scale = transform.localScale;
			scale.x = initialScale * curve.Evaluate(t);
			scale.y = scale.x;
			transform.localScale = scale;
		}
	}

}
















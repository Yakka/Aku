using UnityEngine;
using System.Collections;

public class SeaSurface : MonoBehaviour
{
	public float phase;
	private Vector3 refPos;
	
	void Start ()
	{
		refPos = transform.localPosition;
	}
	
	void Update ()
	{
		Vector3 pos = refPos;
		pos.x += 2f * Mathf.Cos(phase + Time.time);
		pos.y += 2f * Mathf.Sin(phase + 2f * Time.time);
		transform.localPosition = pos;
	}

}



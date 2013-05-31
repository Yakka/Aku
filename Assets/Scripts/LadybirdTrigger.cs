using UnityEngine;
using System.Collections;

public class LadybirdTrigger : MonoBehaviour {
	
	private bool isActivated = false;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other.name == "Drake")
		{
			isActivated = true;
		}
	}
	
	public bool IsActivated()
	{
		return isActivated;
	}
}

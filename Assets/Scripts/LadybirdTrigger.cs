using UnityEngine;
using System.Collections;

public class LadybirdTrigger : MonoBehaviour {
	
	public HiddenPainting hiddenPainting = null;
	public float threshold = 0;
	public int checkedChannel = 0;
	public GameObject objectToEnable;
	private bool isActivated = false;
	private bool hasActivated = false;

	void Update()
	{
		if(objectToEnable != null && !IsHiddenPaintingTrigger() && !hasActivated)
		{
			Helper.SetActive(objectToEnable, true);
			hasActivated = true;
		}
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
	
	public bool IsHiddenPaintingTrigger()
	{
		if(hiddenPainting == null)
		{
			return false;
		}
		else
		{
			if(hiddenPainting.GetRevealRatio(checkedChannel) > threshold)
				return false;
			else
				return true;
		}
	}
}

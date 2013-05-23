using UnityEngine;
using System.Collections;

/// <summary>
/// Enables a game object when a hidden painting is assumed to be complete
/// </summary>
public class HiddenPaintingCompleteTrigger : MonoBehaviour 
{
	public float completeThreshold = 0.8f;
	public int channel = 0;
	public GameObject objectToEnable;
	
	private HiddenPainting hiddenPaintingRef;
	private bool triggered;
	
	void Start ()
	{
		hiddenPaintingRef = gameObject.GetComponent<HiddenPainting>();
		if(hiddenPaintingRef == null)
		{
			Debug.Log("HiddenPaintingCompleteTrigger must be attached to a HiddenPainting !");
		}
		if(objectToEnable == null)
		{
			Debug.Log("You must set a target to HiddenPaintingCompleteTrigger !");
		}
	}
	
	void Update ()
	{
		if(triggered)
			return;

		if(hiddenPaintingRef.GetRevealRatio(channel) > completeThreshold)
		{
			objectToEnable.active = true;
			triggered = true;
			switch(Level.Get.levelID)
			{
			case 1:
				SoundLevel1.Instance.HornetBirth();
				break;
			case 2:
				break;
			}
		}
	}
}


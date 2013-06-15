using UnityEngine;
using System.Collections;

/// <summary>
/// Enables a game object when a hidden painting is assumed to be complete
/// </summary>
public class HiddenPaintingCompleteTrigger : MonoBehaviour 
{
	public float completeThreshold = 0.9f;
	public int channel = 0;
	public GameObject objectToEnable;
	
	private HiddenPainting hiddenPaintingRef;
	private bool triggered;
	
	void Start ()
	{
		hiddenPaintingRef = gameObject.GetComponent<HiddenPainting>();
		if(hiddenPaintingRef == null)
		{
			Debug.LogError("HiddenPaintingCompleteTrigger must be attached to a HiddenPainting !");
		}
		if(objectToEnable == null)
		{
			Debug.LogError("You must set a target to HiddenPaintingCompleteTrigger !");
		}
		
		// If a PaintSpitter is attached to the game object,
		// make thresholds match
		PaintSpitter ps = GetComponent<PaintSpitter>();
		if(ps != null)
			completeThreshold = ps.startThreshold;
	}
	
	void Update ()
	{
		if(triggered)
			return;

		if(hiddenPaintingRef.GetRevealRatio(channel) > completeThreshold)
		{
			Helper.SetActive(objectToEnable, true);

			triggered = true;
			switch(Level.Get.levelID)
			{
			case 1:
				CommonSounds.Instance.HornetBirth();
				break;
			case 2:
				break;
			}
		}
	}
}


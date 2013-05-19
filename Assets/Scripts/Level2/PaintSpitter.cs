using UnityEngine;
using System.Collections;

/// <summary>
/// Must be attached to a game object having a tricolor HiddenPainting behavior.
/// Spits paint particles in a specified direction if the HiddenPainting is revealed.
/// </summary>
public class PaintSpitter : MonoBehaviour
{
	public float startThreshold = 0.85f;
	public Range spawnIntervalSeconds = new Range(0.6f, 1.0f);
	public PaintSpitConfig[] configs = new PaintSpitConfig[3];
	
	private HiddenPainting hiddenPaintingRef;
	private sbyte spitChannel;
	private float nextSpitTime;
	
	// Use this for initialization
	void Start () 
	{
		hiddenPaintingRef = GetComponent<HiddenPainting>();
		if(hiddenPaintingRef == null)
			Debug.LogError("PaintSpitter COULD NOT FIND HiddenPainting ! Object : " + gameObject.name);
		spitChannel = -1;
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Update spit type
		sbyte newSpitChannel = -1; // By default, none will be active
		for(sbyte chnl = 0; chnl < 3; ++chnl)
		{
			// Check each channel until one is widely revealed
			if(hiddenPaintingRef.GetRevealRatio(chnl) > startThreshold)
			{
				newSpitChannel = chnl;
				break;
			}
		}
		spitChannel = newSpitChannel;
		
		// Update spitting
		if(spitChannel != -1) // If a valid channel has been set
		{
			float t = Time.time;
			if(t >= nextSpitTime) // If it's the time to spawn a spit
			{
				// Spawn it
				//Debug.Log("Spit " + spitChannel);
				nextSpitTime = t + spawnIntervalSeconds.Rand();
				Pool.Instance.SpawnPaintSpit(transform.position, configs[spitChannel], spitChannel);
			}
		}
		
	}
	
}


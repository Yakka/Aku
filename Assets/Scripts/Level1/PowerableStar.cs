using UnityEngine;
using System.Collections;

/// <summary>
/// When all these stars get powered, the dragon gets bigger.
/// </summary>
public class PowerableStar : MonoBehaviour
{
	private static ArrayList stars = new ArrayList();
	private static int poweredCount;
	
	private Material material;
	private Color color;
	private bool powered;

	void Start ()
	{
		// Note : the material is expected to have an additive shader
		material = this.GetComponent<MeshRenderer>().material;
		color = material.GetColor("_TintColor");		
		material.SetColor("_TintColor", color * 0.3f);
		stars.Add(this);
		if(Settings.trailerMode)
			powered = true;
	}
	
	void Update ()
	{
		if(powered)
		{
			// Some smooth flickering
			if(powered)
			{
				material.SetColor("_TintColor", 
					color * (0.5f + 0.7f * Mathf.PerlinNoise(10f*Time.time, 0)));
			}
			else
			{
				material.SetColor("_TintColor", 
					color * (0.3f + 0.5f * Mathf.PerlinNoise(10f*Time.time, 0)));
			}
		}
	}
	
	void Power()
	{
		powered = true;
		++poweredCount;
		Debug.Log("Powered " + poweredCount + " / " + stars.Count);
		// TODO feedback
		SoundLevel1.Instance.HitStar(gameObject); //michèle
	}
	
	void OnTriggerEnter (Collider other)
	{
		if(!powered)
		{
			DrakeScale ds = other.GetComponent<DrakeScale>();
			if(ds != null)
			{
				if(ds.ColorLevel > 0)
				{
					Power();
					if(poweredCount == stars.Count)
						ds.drakeRef.Maximize();
				}
			}
		}
	}
	
	void OnTriggerExit (Collider other)
	{
	}

}






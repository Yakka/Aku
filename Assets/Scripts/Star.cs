using UnityEngine;
using System.Collections;

/// <summary>
/// This is a basic star that does absolutely nothing more but flickering.
/// </summary>
public class Star : MonoBehaviour
{
	private Material material;
	private Color color;

	void Start ()
	{
		// Note : the material is expected to have an additive shader
		material = this.GetComponent<MeshRenderer>().material;
		color = material.GetColor("_TintColor");
	}
	
	void Update ()
	{
		// Some smooth flickering
		material.SetColor("_TintColor", 
			color * (0.5f + 0.7f * Mathf.PerlinNoise(10f*Time.time, 0)));
	}
	
	void OnTriggerEnter (Collider other)
	{
	}
	
	void OnTriggerExit (Collider other)
	{
	}

}

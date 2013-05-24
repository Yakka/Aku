using UnityEngine;
using System.Collections;

public class Moon : MonoBehaviour {
	
	
	private float coef = 1f;
	
	private Material material;
	private Color color;
	
	// Use this for initialization
	void Start () {
		// Note : the material is expected to have an additive shader
		material = this.GetComponent<MeshRenderer>().material;
		color = material.GetColor("_TintColor");		
		material.SetColor("_TintColor", color * 0.3f);
	}
	
	// Update is called once per frame
	void Update () {
		material.SetColor("_TintColor", color * (Mathf.Clamp(0.8f + Mathf.PerlinNoise(coef*Time.time, 0), 0.9f, 10f)));
	}
}

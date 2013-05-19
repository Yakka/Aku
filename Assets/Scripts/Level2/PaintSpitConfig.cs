using UnityEngine;
using System.Collections;

[System.Serializable] // To make it editable in Unity
public class PaintSpitConfig
{
	public Color color = new Color(0,0,0,1);
	public float angleDeg;
	public float speed = 10f;
	public Vector2 offset;
}


using System;
using UnityEngine;
using System.Collections.Generic;

class FadingPixel
{
	public float time;
	public float delay;
	public float speed;
	
	public FadingPixel(float delay, float speed)
	{
		time = Time.time;
		this.delay = delay;
		this.speed = speed;
	}

};



using UnityEngine;
using System;

public class DrakeBone
{
	public Vector3 pos;
	public float angleRad;
	public DrakeBone parent; // Next bone to the head
	public DrakeBone child; // Next bone to the tail
	public Drake drakeRef;
	
	public int index; // Position order on the drake
	
	public DrakeBone (float x, float y, float z, int i)
	{
		pos = new Vector3(x, y, z);
		index = i;
	}
	
	public DrakeBone(Vector3 p, int i)
	{
		pos = p;
		index = i;
	}
	
	public float DeltaAngleRad
	{
		get
		{
			if(parent != null)
			{
				return Mathf.DeltaAngle(angleRad, parent.angleRad);
			}
			else if(child != null)
			{
				return Mathf.DeltaAngle(child.angleRad, angleRad);
			}
			else
				return 0;
		}
	}
	
	// TODO multipass update?
	public void Update()
	{
		// This is a basic CCD algorithm, with experimental tweaks.
				
		float dx = parent.pos.x - pos.x;
		float dy = parent.pos.y - pos.y;
		
		float t = Mathf.Atan2(dy, dx); // Returns a angle in radians
		
		// Inertia
		float inertia = 100000.0f * Mathf.Abs(Mathf.DeltaAngle(angleRad, t) / Mathf.PI);
		pos.x += inertia * Mathf.Cos(angleRad) * Time.deltaTime;
		pos.y += inertia * Mathf.Sin(angleRad) * Time.deltaTime;

		//		if(index > 1)
//		{
//			// Torn limiter (does weird things)
//			float da = Mathf.DeltaAngle(parent.angleRad, t);
//			float da_abs = Mathf.Abs(da);
//			const float torn_threshold = 0.1f;
//			if(da_abs > torn_threshold)
//			{
//				da = Mathf.Sign(da) * (torn_threshold + 0.5f*da_abs);
//				t = parent.angleRad + da;
//				//torn_flag = true;
//			}
//		}
		
		// Waving inductor : adds another wave.
		//t += 0.05f * Mathf.Sin(0.1f * i - 2.0f * Time.time);

		angleRad = t;
		pos.x = parent.pos.x - Mathf.Cos(angleRad) * Drake.BONE_SPACING;
		pos.y = parent.pos.y - Mathf.Sin(angleRad) * Drake.BONE_SPACING;
	}


}


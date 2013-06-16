using System;

public struct Vector2i
{
	public int x;
	public int y;
	
	public Vector2i(int x0, int y0)
	{
		x = x0;
		y = y0;
	}
	
	public override int GetHashCode ()
	{
		return x | (y << 16);
	}
	
	public void Set(int x0, int y0)
	{
		x = x0;
		y = y0;
	}

}


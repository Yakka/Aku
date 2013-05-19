using UnityEngine;
using System.Collections;

/// <summary>
/// Paints pixels on textures attached on tiles.
/// </summary>
public class PixelPainter
{
	public const float MOON_BRUSH_SIZE = 0.4f;
	public const float MOON_PAINT_FADE_DELAY = 5f; // in seconds before fading to take effect
	public const float MOON_PAINT_FADE_SPEED = 1.5f; // in [0,1] color units / second
	
	public const float NORMAL_PAINT_FADE_SPEED = 3.0f; // in [0,1] color units / second
	public const float NORMAL_BRUSH_SIZE = 0.7f;

	// TODO Brush
	public HiddenPainting hiddenPaintingRef;
	public Color color;
	public bool moonPaint;
	private float brushSize = NORMAL_BRUSH_SIZE;
	//private float prevX;
	//private float prevY;

	public void SetBrush(float size)
	{
		brushSize = size;
	}
	
	public void Update (float x0, float y0)
	{
		if(color.a < 0.01f)
			return;
		
		// TODO paint a line between last and new position, instead of a simple dot
		
		float bs = brushSize;
		if(moonPaint)
			bs = MOON_BRUSH_SIZE;
		
		Color pcolor = new Color(); // Color that will finally be set
		Color pix;
		int texX, texY;

		float bw = 0.6f; // Brush bristle spacing
		float x, y;
		float minX = x0 - bs;
		float minY = y0 - bs;
		float maxX = x0 + bs;
		float maxY = y0 + bs;
		int channel;
		
		Tile tile;
		
		//Profiler.BeginSample("Painting");
		for(x = minX; x < maxX; x += bw)
		{
			for(y = minY; y < maxY; y += bw)
			{
				tile = Level.Get.GetTileFromWorldCoords(x, y);
				
				if(tile.IsPaintable)
				{
					tile.WorldToTextureCoords(x, y, out texX, out texY);
					
					// Color blending
					pix = tile.GetPaintPixel(texX, texY);
					pcolor.r = color.r;
					pcolor.g = color.g;
					pcolor.b = color.b;
					pcolor.a = Mathf.Max(pix.a, color.a);
					
					if(hiddenPaintingRef != null)
					{
						tile.SetPaintPixelNoFade(pcolor, texX, texY);
						/* // Fading is no longer useful here, use shader instead
						channel = hiddenPaintingRef.GetMatchingChannelFromWorldCoords(x, y, pcolor);
						if(channel != -1)
						{
							// Hidden painting (fading pixels)
							tile.SetPaintPixel(pcolor, texX, texY, 0, NORMAL_PAINT_FADE_SPEED);
						}
						else
						{
							// Standard painting
							tile.SetPaintPixelNoFade(pcolor, texX, texY);
						}
						hiddenPaintingRef.CheckPixel(texX, texY, (sbyte)channel);
						*/
					}
					else if(moonPaint)
					{
						// Moon painting
						// Note : legacy software fading is used 
						// because shaders doesn't support it yet
						tile.SetPaintPixel(pcolor, texX, texY, 
							MOON_PAINT_FADE_DELAY, 
							MOON_PAINT_FADE_SPEED);
					}
					else
					{
						// Standard painting without hidden ink
						tile.SetPaintPixelNoFade(pcolor, texX, texY);
					}
				}
			}
		}
		//Profiler.EndSample();
		
		//prevX = x0;
		//prevY = y0;
	}

}



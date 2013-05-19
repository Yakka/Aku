using UnityEngine;
using System.Collections;

public class HiddenPainting : MonoBehaviour
{
	const float COLOR_TOLERANCE = 0.5f;
	const float MATCH_THRESHOLD = 0.3f;

	/// <summary>
	/// This texture compiles up to 3 greyscale masks over the 3 color RGB channels
	/// </summary>
	public Texture2D texture;
	public bool debug;
	
	/// <summary>
	/// Describes which actual color match the 3 channels in the texture
	/// </summary>
	public Color[] channelMapping; // RGB
	
	private Vector3 pos; // Faster shortcut to transform.position
	
	#region "Pixel counting"
	
	public bool countPixels; // If set to true, enables pixel counting
	public int divSize = 4; // In pixels / division
	
	// To remind which divisions have been revealed :
	// -2 : the division is not relevant,
	// -1 : can be revealed but is not,
	// >0 : revealed, and matches the corresponding channel index
	private int[,] revealMatrix;
	private int revealMatrixW;
	private int revealMatrixH;
	
	private int[] divCount; // Count of relevant divisions in the texture, per channel
	private int[] checkedDivsCount; // How many divisions have been revealed
	
	#endregion
	
	//=========================================
	
	void Awake()
	{
		pos = transform.position;
	}
	
	void Start ()
	{
//		MeshRenderer r = gameObject.GetComponent<MeshRenderer>();
//		r.enabled = false;
		
		if(countPixels)
		{
			BuildRevealMatrix();
		}
	}
	
	private void WorldToDivCoords(float x, float y, out int divX, out int divY)
	{
		int texX, texY;		
		WorldToPaintingCoords(x, y, out texX, out texY);
		divX = texX / divSize;
		divY = texY / divSize;
	}
	
	private void BuildRevealMatrix()
	{
		Debug.Log("Building reveal matrix for " + gameObject.name);
		
		revealMatrixW = texture.width/divSize;
		revealMatrixH = texture.height/divSize;		
		revealMatrix = new int[revealMatrixW, revealMatrixH];
		divCount = new int[3];
		checkedDivsCount = new int[3];
		
		// in [0,1] (similar to 1 - photoshopMagicStickTolerance).
		// The upper it is, the less pixels will be counted.
		float tolerance = 0.5f;

		int relevanceThreshold = 1; // At least 1 pixel
		int[] divRelevantPixels = new int[3];
		int x0, y0;
		
		// For each division in the reveal matrix
		for(int divY = 0; divY < revealMatrixH; ++divY)
		{
			for(int divX = 0; divX < revealMatrixW; ++divX)
			{
				// Count relevant pixels in the division
				
				for(int i = 0; i < divRelevantPixels.Length; ++i)
					divRelevantPixels[i] = 0;
				
				x0 = divX * divSize;
				y0 = divY * divSize;
				
				// For each pixel in the division
				for(int y = 0; y < divSize; ++y)
				{
					for(int x = 0; x < divSize; ++x)
					{
						// Count relevant pixels from the 3 channels :
						Color pix = texture.GetPixel(x0+x, y0+y);
						divRelevantPixels[0] += pix.r > tolerance ? 1 : 0;
						divRelevantPixels[1] += pix.g > tolerance ? 1 : 0;
						divRelevantPixels[2] += pix.b > tolerance ? 1 : 0;
					} 
				}
				
				// If at least 1 pixel is relevant in the div, the div will be counted
				divCount[0] += divRelevantPixels[0] >= relevanceThreshold ? 1 : 0;
				divCount[1] += divRelevantPixels[1] >= relevanceThreshold ? 1 : 0;
				divCount[2] += divRelevantPixels[2] >= relevanceThreshold ? 1 : 0;
				
				if(divRelevantPixels[0] < relevanceThreshold 
					&& divRelevantPixels[1] < relevanceThreshold 
					&& divRelevantPixels[2] < relevanceThreshold)
				{
					revealMatrix[divX, divY] = -2; // Not revealable
				}
				else
					revealMatrix[divX, divY] = -1; // Not revealed
			}
		}
		
		/* // Was useful when using pixel-matching divSize
		// Because paintTextures and hiddenPainting doesn't have the same size,
		// it's required to multiply the count by a ratio
		// (Counts are made with the hidden texture and will be modified from a paint texture)
		float htexArea = texture.width*texture.height;
		float ptexArea = Tile.PAINT_TEXTURE_WIDTH*Tile.PAINT_TEXTURE_HEIGHT;
		float k = ptexArea / htexArea;
		divCount[0] = (int)((float)divCount[0]*k);
		divCount[1] = (int)((float)divCount[1]*k);
		divCount[2] = (int)((float)divCount[2]*k);
		*/
		
		if(debug)
		{
			Debug.Log("Div count for " + gameObject.name + ":");
			Debug.Log(revealMatrixW + " * " + revealMatrixH);
			Debug.Log("-- R: " + divCount[0] + " / " + revealMatrixW*revealMatrixH);
			Debug.Log("-- G: " + divCount[1] + " / " + revealMatrixW*revealMatrixH);
			Debug.Log("-- B: " + divCount[2] + " / " + revealMatrixW*revealMatrixH);
		}
	}
	
	public void CheckReveal(float worldX, float worldY, int channel)
	{
		if(countPixels)
		{
			int divX, divY;
			WorldToDivCoords(worldX, worldY, out divX, out divY);
			
			// Are coordinates valid?
			if(divX >= 0 && divY >= 0 && divX < revealMatrixW && divY < revealMatrixH)
			{
				CheckRevealDivCoords(divX, divY, channel);
			}
		}
	}
	
	private void CheckRevealDivCoords(int divX, int divY, int channel)
	{
		int oldChannel = revealMatrix[divX, divY];
		
		// If the division can be revealed and has a different channel
		if(oldChannel != -2 && oldChannel != channel)
		{
			if(oldChannel != -1) 
			{
				--checkedDivsCount[oldChannel];
			}
			if(channel != -1)
			{
				++checkedDivsCount[channel];
			}
			revealMatrix[divX, divY] = channel;
		}
	}
	
	public void CheckRevealZone(float centerWorldX, float centerWorldY, float radius, int channel)
	{
		if(countPixels)
		{
			int minDivX, minDivY, maxDivX, maxDivY;
			float f = 0.75f; // A square is used instead of a circle. It's faster and easier.
			WorldToDivCoords(centerWorldX-f*radius, centerWorldY-f*radius, out minDivX, out minDivY);
			WorldToDivCoords(centerWorldX+f*radius, centerWorldY+f*radius, out maxDivX, out maxDivY);
			//Debug.Log("CheckRevealZone " + minDivX + ", " + minDivY + ", " + maxDivX + ", " + maxDivY);
			
			minDivX = Mathf.Clamp(minDivX, 0, revealMatrixW-1);
			minDivY = Mathf.Clamp(minDivY, 0, revealMatrixH-1);
			maxDivX = Mathf.Clamp(maxDivX, 0, revealMatrixW-1);
			maxDivY = Mathf.Clamp(maxDivY, 0, revealMatrixH-1);
			
			// Check everything that is in the square
			for(int y = minDivY; y <= maxDivY; ++y)
			{
				for(int x = minDivX; x <= maxDivX; ++x)
				{
					CheckRevealDivCoords(x, y, channel);
				}
			}
		}
	}
	
	/*public void CheckPixel(int texX, int texY, int channel)
	{
		if(countPixels)
		{
			int oldChannel = revealMatrix[texX, texY];
			if(channel != oldChannel)
			{
				if(oldChannel != -1)
					--checkedDivsCount[oldChannel];
				if(channel != -1)
					++checkedDivsCount[channel];
				revealMatrix[texX, texY] = channel;
			}
		}
	}*/
	
	/// <summary>
	/// Computes the reveal ratio of a given channel of the hidden painting.
	/// For example, 0.7f means that the hidden painting located on the given 
	/// channel has been revealated by 70%.
	/// </summary>
	/// <returns>The reveal ratio in [0,1]. 0 if pixel count is disabled.</returns>
	/// <param name='channel'>
	/// Channel to read. There is 3 channels, for R, G and B masks.
	/// The default channel is 0 (R).
	/// </param>
	public float GetRevealRatio(int channel)
	{
		if(countPixels)
			return Mathf.Clamp01((float)checkedDivsCount[channel] / (float)divCount[channel]);
		return 0;
	}
	
	void WorldToPaintingCoords(float x, float y, out int texX, out int texY)
	{
		texX = (int)((float)texture.width * (x - pos.x) / transform.localScale.x);
		texY = (int)((float)texture.height * (y - pos.y) / transform.localScale.y);
	}
	
	public bool IsTricolor
	{
		get { return channelMapping != null && channelMapping.Length == 3; }
	}
	
//	int __n = 0;
	
	/// <summary>
	/// Tests and returns wether a color should be faded by invisible ink or not.
	/// </summary>
	/// <returns>
	/// -1 if no fade, the channel index otherwise (0, 1 or 2)
	/// </returns>
	/// <param name='worldX'>X in world coordinates.</param>
	/// <param name='worldY'>Y in world coordinates.</param>
	/// <param name='color'>Color that is used to paint</param>
	public int GetMatchingChannelFromWorldCoords(float worldX, float worldY, Color color)
	{
		int texX, texY;
		WorldToPaintingCoords(worldX, worldY, out texX, out texY);
		//Debug.Log(texX + ", " + texY);
		if(texX >= 0 && texY >= 0 && texX < texture.width && texY < texture.height)
		{
			if(IsTricolor)
			{
				// TODO index colors for the second level, and forget about voodoo color matching
				// RGB mapped channels are used
				
				Color pix = texture.GetPixel(texX, texY);
				
				float redMatch = 	Helper.ColorMatch(color, pix.r * channelMapping[0], COLOR_TOLERANCE);
				float greenMatch = 	Helper.ColorMatch(color, pix.g * channelMapping[1], COLOR_TOLERANCE);
				float blueMatch = 	Helper.ColorMatch(color, pix.b * channelMapping[2], COLOR_TOLERANCE);
				
				if(redMatch > greenMatch)
				{
					if(redMatch > blueMatch)
						return 1f-redMatch < MATCH_THRESHOLD ? 0 : -1;
					else
						return 1f-blueMatch < MATCH_THRESHOLD ? 2 : -1;
				}
				else
				{
					if(blueMatch > greenMatch)
						return 1f-blueMatch < MATCH_THRESHOLD ? 2 : -1;
					else
						return 1f-greenMatch < MATCH_THRESHOLD ? 1 : -1;
				}
				
//				__n++; // For debug
//				if(__n%400 == 0)
//					Debug.Log(m);	
				
//				return 1f-m;
			}
			else // No tricolor mapping
			{
				// Red channel is used as greyscale
				return 1f-texture.GetPixel(texX, texY).r < MATCH_THRESHOLD ? 0 : -1;
			}
		}
		return -1;
	}
		
	// OLD CODE
//	public float GetMaskFromWorldCoords(float worldX, float worldY)
//	{
//		int texX, texY;
//		WorldToPaintingCoords(worldX, worldY, out texX, out texY);
//		//Debug.Log(texX + ", " + texY);
//		if(texX >= 0 && texY >= 0 && texX < texture.width && texY < texture.height)
//		{
//			//Color pix = texture.GetPixel(texX, texY);
//			return texture.GetPixel(texX, texY).r;
//		}
//		return 1;
//	}
	
	void OnGUI()
	{
		if(Settings.debugMode && debug)
		{
			if(countPixels)
			{
				int a = (int)(100f * (float)checkedDivsCount[0] / (float)divCount[0]);
				int b = (int)(100f * (float)checkedDivsCount[1] / (float)divCount[1]);
				int c = (int)(100f * (float)checkedDivsCount[2] / (float)divCount[2]);
				int ta = 100;
				int tb = 100;
				int tc = 100;
				
//				int a = checkedDivsCount[0];
//				int b = checkedDivsCount[1];
//				int c = checkedDivsCount[2];
//				int ta = divCount[0];
//				int tb = divCount[1];
//				int tc = divCount[2];
				
				GUI.Label(new Rect(16, 380, 256, 332), 
					"Reveal : " 
					+ a + "/" + ta + ", "
					+ b + "/" + tb + ", "
					+ c + "/" + tc, 
					Settings.debugGuiStyle);
			}
		}
	}
	
}



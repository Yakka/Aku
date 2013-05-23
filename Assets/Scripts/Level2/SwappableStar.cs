using UnityEngine;
using System.Collections;

public class SwappableStar : MonoBehaviour
{
	private const float SWAP_SPEED = 0.5f;
	
	public GameObject cloudObj; // The associated paint ball
	private Cloud cloud;
	
	private Material material;
	private Color color;
	
	private bool swapping;
	private Vector3 srcPos;
	private Vector3 dstPos;
	private float swapProgress;

	void Start ()
	{
		// Note : the material is expected to have an additive shader
		material = this.GetComponent<MeshRenderer>().material;
		color = material.GetColor("_TintColor");
		
		SetCloud(cloudObj);
		
		// TODO random rotation
	}
	
	private void SetCloud(GameObject obj)
	{
		cloudObj = obj;
		cloud = cloudObj.GetComponent<Cloud>();
		if(cloud == null)
			Debug.LogError("SwappableStar: CLOUD IS NOT A CLOUD !");
	}
	
	void Update ()
	{
		// Flickering
		material.SetColor("_TintColor", 
			color * (0.5f + 0.7f * Mathf.PerlinNoise(10f*Time.time, 0)));
		
		// Swapping animation
		if(swapping)
		{
			swapProgress += SWAP_SPEED * Time.deltaTime;
			if(swapProgress >= 1f)
			{
				// Finished
				swapProgress = 1f;
				transform.position = dstPos;
				swapping = false;
				Level.Get.Attach(gameObject); // Don't forget to reattach
			}
			else
			{
				// TODO fancier animation (currently linear)
				transform.position = Vector3.Lerp(srcPos, dstPos, swapProgress);
			}
		}
	}
	
	void OnTriggerEnter (Collider other)
	{
		if(!swapping)
		{
			if(other.name.Contains("drake_scale"))
			{
				DrakeScale drakeScale = other.GetComponent<DrakeScale>();
				Drake drake = drakeScale.drakeRef;
				
				if(drake.LastTouchedStar != null)
				{
					SwappableStar star = 
						drake.LastTouchedStar.GetComponent<SwappableStar>();

					// If the star exist and is different
					if(star != null && star != this)
					{
						drake.LastTouchedStar = null;
						Swap(star, true);
						SoundLevel2.Instance.SwapStar();
					}
				}
			}
		}
	}
	
	//void OnTriggerExit (Collider other) {}
	
	protected void Swap(SwappableStar other, bool swapBack)
	{
		// Change paint balls
		if(!swapBack)
		{
			// Swap colors
			cloud.SwapColors(other.cloud);
			// Swap objects
			GameObject otherCloudObj = other.cloudObj;
			other.SetCloud(this.cloudObj);
			SetCloud(otherCloudObj);
		}
		
		// Initialize animation
		srcPos = transform.position;
		dstPos = other.transform.position;
		swapping = true;
		swapProgress = 0;
		
		// Trigger swapping on the other star
		if(swapBack)
			other.Swap(this, false);
		
		// Detach the star during the animation
		Level.Get.Detach(gameObject);
	}
	
	public bool IsSwapping
	{
		get { return swapping; }
	}

}





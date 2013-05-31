using UnityEngine;
using System.Collections;

public class DrakeScale : MonoBehaviour
{
	public static float COLOR_DISTANCE_MIN = 100f;
	public static float COLOR_DISTANCE_MAX = 400f;
	private static Range brushSize = new Range(0.1f, 1.75f);
	
	public float parametricPosition; // 0 means the head, 1 means the tail
	public int indexedPosition;
	public Drake drakeRef;
	public DrakeBone boneRef; // The bone which this scale is attached to
	public bool isXRotative = true; // Will the scale rotate around the body?
	public Material materialRef;	
	private float xRotationDeg = 0; // Rotation around the body
	private float xRotationDegVar;
	private float spawnShine;
	public GameObject glowRef;
	
	private bool isUnderwater;

	private PainterBehavior pb;
	//private float nextBleedTime;

	// Use this for initialization
	void Start ()
	{
		gameObject.AddComponent<SphereCollider>();
		// For collision to be detected, one of the objects must have a rigidbody
		Rigidbody rigidBody = gameObject.AddComponent<Rigidbody>();
		rigidBody.isKinematic = true; // But I make it kinematic so it won't simulate physics on itself
		
		if(isXRotative)
			xRotationDegVar = Random.Range(-90, 90);
		
		pb = GetComponent<PainterBehavior>();
		//pb.materialRef = materialRef; // Dont link it anymore, because it breaks batching
		
		pb.SetBrush(brushSize.Lerp(parametricPosition));
		
		UpdateColorDistanceAutonomy();
		
		// Halos?
	}
	
	public void UpdateColorDistanceAutonomy()
	{
		float gr = drakeRef.GetGrowthRatio();
		float cd = (1f-gr)*COLOR_DISTANCE_MIN + gr * COLOR_DISTANCE_MAX;
		if(pb == null)
			pb = GetComponent<PainterBehavior>();
		pb.distanceAutonomy = cd;
	}
	
	public void Shine()
	{
		spawnShine = 1;
	}
	
	// Update is called once per frame
	void Update ()
	{
		//================ Color charge and paint =======================
		
		bool wasUnderwater = isUnderwater;
		isUnderwater = Level.Get.IsWater(transform.position.x, transform.position.y);
		if(isUnderwater && !wasUnderwater)
		{
			pb.ColorLevel = 0;
		}
		
		if(!HasMoonPaint && ColorLevel > 0.2f && Mathf.Abs(boneRef.DeltaAngleRad) > 0.07f)
		{
			if(Random.Range(0f, 1f) < 0.01f)
			{
				//Debug.Log("DRIP");
				if(Level.Get.levelID == 2)
				{
					PolyPaintManager.Instance.SpawnDrip(
						pb.ColorIndex, transform.position.x, transform.position.y);
				}
				else
				{
					PolyPaintManager.Instance.SpawnDrip(
						Color, transform.position.x, transform.position.y);
				}
			}
		}
		
		// Paint bleeds
//		if(ColorLevel > 0.001f)
//		{
//			if(Time.time > nextBleedTime)
//			{
//				PaintBleedPool.Instance.Spawn(transform.position, Color, ColorLevel);
//				nextBleedTime = Time.time + Random.Range(0.2f, 0.4f);
//			}
//		}
		
		//================ Movement and animation ==============
		
		transform.position = boneRef.pos;
		
		if(isXRotative)
		{
			xRotationDegVar += Time.deltaTime * Random.Range(-90f, 90f);
			
			xRotationDeg += xRotationDegVar * Time.deltaTime;
			if(xRotationDeg > 180) // wrap angle
				xRotationDeg -= 360f;
		}
		
		transform.rotation = Quaternion.Euler(xRotationDeg, 0f, 0f); // Here in degrees...
		transform.RotateAround(Vector3.forward, boneRef.angleRad+Mathf.PI); // And here in radians. What the fuck
		
		//=================== Shine ========================
		
		if(glowRef != null)
		{
			glowRef.transform.position = 
				new Vector3(boneRef.pos.x, boneRef.pos.y, boneRef.pos.z + 1);
		}
		
		// Note: This is just a fancy thing, but it currently increases draw calls by ~20.
		// So if we didn't referenced the scale material to avoid this, just do nothing.
		if(materialRef != null && spawnShine > 0)
		{
			spawnShine -= 2f * Time.deltaTime;
			if(spawnShine < 0)
				spawnShine = 0;
			
			materialRef.color = Color.Lerp(
				pb != null ? pb.Color : Color.black, Color.white, spawnShine);
		}
	}
	
//	void OnTriggerEnter(Collider other)
//	{
//		SwappableStar swappableStar = null;
		
//		if((swappableStar = other.GetComponent<SwappableStar>()) != null)
//		{
//			if(!swappableStar.IsSwapping)
//				drakeRef.OnTouchedStar(other.gameObject);
//		}
//	}
	
	void OnTriggerExit(Collider other)
	{
		if(other.GetComponent<Cloud>() != null || other.name == "Moon")
		{
			pb.ColorLevel = 1f; // Set color as charged
		}
	}

	public Color Color
	{
		get { return pb.Color; }
	}
	
	public int ColorIndex
	{
		get { return pb.ColorIndex; }
	}
	
	public float ColorLevel
	{
		get { return pb != null ? pb.ColorLevel : 0; }
		set
		{
			if(pb != null)
			{
				pb.ColorLevel = 0;
			}
		}
	}
	
	public bool HasColor
	{
		get { return pb != null ? pb.HasColor : false; }
	}
	
	public bool HasMoonPaint
	{
		get { return pb != null ? pb.HasMoonPaint : false; }
	}

}




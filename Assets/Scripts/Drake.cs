using UnityEngine;
using System.Collections;
using System;

public class Drake : MonoBehaviour
{
	#region "Constants"
	
	private const float STUN_DURATION = 1f;
	public const float BONE_SPACING = 0.7f;
	private const float SCALE_SPACING = 1f;
	private const int SCALE_COUNT_MAX = 40;
	private const int BONE_COUNT_MAX = 100;
	private const int BONE_COUNT_MIN = 50; // Just to have a minimum
	private const float DASH_ADDSPEED = 15;
	private const float GRAB_ROTATION_RANDOM = 30f;
	private const float SPLASH_TRIGGER_ANGSPEED_DEG = 140;
	private const float SPLASH_TRIGGER_DELAY_S = 1.0f;
	
	#endregion
	
	private static Drake globalInstance;
	
	#region "Editor config"
	
	public Range speedLimit; // Default minimal and maximum air speed of the dragon
	public float acceleration;
	public Range rotationAbility; // How fast the drake can turn, depending on the speed
//	public float rotationAbility = 0.2f; // How fast the drake can turn
	public float randomDeviation = 0.5f; // Coefficient of the random rotation
	public float periodicSineDeviation = 1f; // Coefficient of the sinusoidal periodic rotation
	public float wavingToSpeedAcceleration = 1f; // Acceleration coefficient of ondulation
	public float linearSpeedReduction; // Speed reduction per second
	public float quadraticSpeedReduction; // Quadratic speed reduction per second
	public float drakeGravity = 1f;
//	public int angleSinergy = 20; // The difference of angle which allows an acceleration
	public float wavingPhaseSpeedMax = 3f; // Size of the wave
	public float touchSpeedCoeff = 2f; // Speed coefficient
	
	public GameObject[] scalePrefabs; // Which prefabs are used to make the scales
	public GameObject tailPrefab;
	public Material scaleMaterial;
	public GameObject glowPrefab;
	
	#endregion
	#region "Position, speed and rotation"
	
	private float speed; // Actual dragon speed in world units / seconds
	private float targetSpeed; // Speed the drake have to stabilize to.
	private float headRotationDeg; // Current un-normalized head angle.
	private float lastFrameHeadRotationDeg;
	private float targetHeadRotationDegNorm; // Normalized angle the drake have to stabilize on.
	
	private float wavingPhase; // Current phase of waving
	private float wavingPhaseSpeed; // Speed of the wave
	public float seaCoef = 0.1f; // Speed coef when the drake is in the sea
	
	#endregion	
	#region "Gameplay and interactions"
	
	private bool lastFrameUnderwater; // Did the head was into water on the last frame?
	private GameObject lastTouchedStar;
	private bool grabbedByHornets = false;
	// The drake is not controllable when it is stunned.
	public float unstunTime; // Timestamp after which the drake will not be stunned
	private float splashTriggerTime; // Last timestamp where the drake were not able to do a splash
	
	#endregion
	#region "Scales, bones, body line and tail"
	
	private DrakeBone[] bones; // Animation
	private int startBoneCount;
	public int currentBoneCount = 80; // How many bones are currently used for the animation

	private DrakeScale[] scales;
	//private int startScaleCount; // How many scales we had on start
	private int currentScaleCount; // Current scale count, != scale.Length (which is the max)
	private LineRenderer lineRenderer; // the core line
	private GameObject tail;
	
	#endregion
	
	//
	// Methods
	//
	
	void Awake()
	{
		globalInstance = this;
	}
	
	public static Drake Instance
	{
		get { return globalInstance; } 
	}
	
	void Start ()
	{
		#region "Check parameters"
		
		targetSpeed = speedLimit.min;
		speed = targetSpeed;
		wavingPhaseSpeed = wavingPhaseSpeedMax;
		// Default parameters
		if (currentBoneCount <= BONE_COUNT_MIN)
			currentBoneCount = BONE_COUNT_MIN;
		
		#endregion
		#region "Create bones"
		
		// Pre-allocate the maximum number of bones
		bones = new DrakeBone[BONE_COUNT_MAX];
		bones [0] = new DrakeBone (transform.position, 0);
		for (int i = 1; i < bones.Length; i++)
		{
			DrakeBone prev = bones [i - 1];
			
			// Align as a straight line
			DrakeBone bone = new DrakeBone (
				prev.pos.x - BONE_SPACING, prev.pos.y, prev.pos.z, i);
			bone.angleRad = 0;
			
			// Linking
			bone.drakeRef = this;
			bone.parent = prev;
			prev.child = bone;
			
			bones [i] = bone;
		}
		
		#endregion
		#region "Create body line"
		
		CreateBodyLine();
		
		#endregion
		#region "Create scales"
		
		// Pre-allocate the maximum number of scales
		// (only a subset of them will be active)
		scales = new DrakeScale[SCALE_COUNT_MAX];
		float t_step = 1.0f / (float)scales.Length;
		
		for (int i = 0; i < scales.Length; ++i)
		{
			float t = t_step * i;
			
			// Get the bone where we will attach the scale
			DrakeBone bone = GetBoneFromParametric (t);
			
			// Compute a random scale ID
			int prefabIndex = UnityEngine.Random.Range (0, scalePrefabs.Length - 1);

			// Create scale
			GameObject scaleObject = Instantiate (
				scalePrefabs [prefabIndex],
				new Vector3 (bone.pos.x, bone.pos.y, bone.pos.z),
				Quaternion.identity
			) as GameObject;
			
			// Set its color
			//Material scaleMaterial = new Material (Shader.Find ("Diffuse"));
			//scaleMaterial.color = Color.black;
			MeshRenderer meshRenderer = scaleObject.GetComponent<MeshRenderer> ();
			meshRenderer.material = scaleMaterial;

			// Add scripts
			scaleObject.AddComponent("DrakeScale");
			scaleObject.AddComponent("PainterBehavior");
			
			// Add glow
			GameObject glowObj = null;
			if(i != 0) { // because the first scale is... well, not used (See below).
				glowObj = Instantiate(glowPrefab) as GameObject;
			}
			
			// Get scale script and init some of its variables
			DrakeScale scaleScript = scaleObject.GetComponent<DrakeScale> ();
			scaleScript.boneRef = bone;
			scaleScript.drakeRef = this;
			scaleScript.glowRef = glowObj;
			scaleScript.parametricPosition = t;
			scaleScript.indexedPosition = i;
			//scaleScript.materialRef = scaleMaterial;
			scales[i] = scaleScript;
			
			// Fix scale size
			const float scaleFix = 1.15f;
			scaleObject.transform.localScale = new Vector3 (scaleFix, scaleFix, scaleFix);
			
			// Set scale as child of the drake : not useful for now	
			
		}
		
		// Disable first scale because it overlaps on the head
		// TODO dirty code, try to do this more cleanly
		scales[0].gameObject.active = false;
				
		#endregion
		#region "Create tail"

		DrakeBone lastBone = bones [currentBoneCount - 1];
		tail = Instantiate (
			tailPrefab,
			new Vector3 (lastBone.pos.x, lastBone.pos.y, lastBone.pos.z),
			Quaternion.identity
		) as GameObject;
		
		// Set tail mesh color
		Material tailMaterial = new Material (Shader.Find ("Diffuse"));
		tailMaterial.color = Color.black;
		tail.GetComponent<MeshRenderer>().material = tailMaterial;

		#endregion
		
		ChangeBoneCount(currentBoneCount);
		startBoneCount = currentBoneCount;
		//startScaleCount = currentScaleCount;
		
		if(Settings.trailerMode)
			ChangeBoneCount(BONE_COUNT_MAX);
	}
	
	/// <summary>
	/// Changes the length of the drake by modifying the last bone index
	/// </summary>
	/// <param name='newCount'>
	/// New bone count (superior bones will be disabled)
	/// </param>
	private void ChangeBoneCount(int newCount)
	{
		// Change index of last bone
		currentBoneCount = Mathf.Clamp(newCount, BONE_COUNT_MIN, bones.Length);
		
		// Update line renderer
		CreateBodyLine();
		
		// Update scales
		int cnt = 0;
		for(int i = 0; i < scales.Length; ++i)
		{
			DrakeScale s = scales[i];
			if(s.boneRef.index < currentBoneCount && i != 0)
			{
				if(!scales[i].gameObject.active)
				{
					scales[i].gameObject.active = true;
					if(scales[i].glowRef != null)
						scales[i].glowRef.active = true;
					s.Shine();
					// Note : they will be updated in their Start() method
					//s.UpdateColorDistanceAutonomy();
				}
				else
					s.UpdateColorDistanceAutonomy();
				++cnt;
			}
			else
			{
				scales[i].gameObject.active = false;
				if(scales[i].glowRef != null)
					scales[i].glowRef.active = false;
			}
		}
		
		currentScaleCount = cnt;
		
		// Update tail position
		DrakeBone lastBone = bones [currentBoneCount - 1];
		tail.transform.position = lastBone.pos;
	}
	
	/// <summary>
	/// Enlarges the dragon for free (without creating new objects).
	/// </summary>
	public void Maximize()
	{
		ChangeBoneCount(BONE_COUNT_MAX);
		// TODO audio feedback
		SoundLevel1.Instance.EnlargeDrake(); 
	}
	
	/// <summary>
	/// Creates or updates the body line renderer from the bones.
	/// </summary>
	private void CreateBodyLine()
	{
		// Get or create line renderer
		lineRenderer = gameObject.GetComponent<LineRenderer>();
		if(lineRenderer == null)
			lineRenderer = gameObject.AddComponent<LineRenderer> ();
		
		lineRenderer.material = new Material (Shader.Find ("Particles/Alpha Blended"));
		lineRenderer.SetColors (Color.black, Color.black);
		lineRenderer.SetWidth (0.55f, 0.3f);
		lineRenderer.SetVertexCount (currentBoneCount);
		lineRenderer.castShadows = false;
		for (int i = 0; i < currentBoneCount; i++)
		{
			lineRenderer.SetPosition (i, bones[i].pos);
		}
	}
	
	private DrakeBone GetBoneFromParametric (float t)
	{
		// Note : Assumes that each bones of the body are equidistant.
		// Note 2 : doesn't computes an inter-bone position
		int i = (int)(t * (float)bones.Length);
		return bones [Mathf.Clamp (i, 0, bones.Length - 1)];
	}
	
	public void OrientateTo (Vector3 target)
	{
		if(IsStunned)
			return;
		Vector2 v = new Vector2 (target.x - transform.position.x, target.y - transform.position.y);
		v.Normalize ();
		this.targetHeadRotationDegNorm = Mathf.Atan2 (v.y, v.x) * Mathf.Rad2Deg;
		//Debug.Log("ORIENTATE " + targetHeadRotationDegNorm);
	}
	
	public void Dash()
	{
		if(IsStunned)
			return;
		this.targetSpeed = speedLimit.Clamp(targetSpeed + DASH_ADDSPEED);
	}
	
	void Stun(float stunDuration)
	{
		unstunTime = Time.time + stunDuration;
	}
	
	public bool IsStunned
	{
		get { return Time.time < unstunTime; }
	}
	
	/// <summary>
	/// Computes the middle position of the drake,
	/// from all its visible scales.
	/// Note : it can be outside of it, because it's a barycenter.
	/// </summary>
	/// <returns>
	/// The barycenter.
	/// </returns>
	public Vector3 GetBarycenter()
	{
		Vector3 sum = new Vector3(0,0,0);
		for(int i = 0; i < currentScaleCount; ++i)
		{
			sum.x += scales[i].transform.position.x;
			sum.y += scales[i].transform.position.y;
			sum.z += scales[i].transform.position.z;
		}
		return sum / (float)currentScaleCount;
	}
	
	void Update ()
	{
		if(!Settings.onTablet)
		{
			// This is just for debug. It enlarges the dragon (for free)
			if(Input.GetKeyDown(KeyCode.Space))
				ChangeBoneCount(BONE_COUNT_MAX);
		}

		#region "Head update"
		
		#region "Head speed"
		
		// Gravity :
		// The drake has more difficulty to go up,
		// and is helped by gravity when going down
		/*float gravityModifier = Vector3.Dot(Vector3.down, transform.right);
		if(gravityModifier > 0)
		{
			targetSpeed += gravityModifier * drakeGravity * Time.deltaTime;
		}*/
		
		// Speed addition with drake ondulations 
		float wavingSpeedAddition = 
			wavingToSpeedAcceleration * Mathf.Cos(wavingPhase) * speedLimit.Mid * Time.deltaTime;
		if(wavingSpeedAddition > 0)
			targetSpeed += wavingSpeedAddition;
		
		// Speed reduction by air friction
		targetSpeed -= linearSpeedReduction * Time.deltaTime;
		targetSpeed -= quadraticSpeedReduction * targetSpeed * targetSpeed * Time.deltaTime;
		bool diving = false;
		// Effects of the sea
		if(Level.Get.IsWater(transform.position.x, transform.position.y))
		{
			// Speed is slowing
			// TODO maybe put this in friction code?
			//
			
			if(!lastFrameUnderwater)
			{
				if(transform.right.y < 0)
				{
					OnSeaDive();
					diving = true;
				}
				lastFrameUnderwater = true;
			}
		}
		else
		{
			if(lastFrameUnderwater)
			{
				if(transform.right.y > 0)
					OnSeaSurface();
				lastFrameUnderwater = false;
			}
		}
		
		#endregion
		#region "Head rotation and waving"
		
		// Update phase
		if(wavingPhaseSpeed < wavingPhaseSpeedMax)
			wavingPhaseSpeed += 2f * Time.deltaTime;
		wavingPhase += wavingPhaseSpeed * Time.deltaTime;
		
		// Sine waving
		if(periodicSineDeviation != 0)
			headRotationDeg += periodicSineDeviation * Mathf.Sin (wavingPhase);
		
		// Noise waving
		if(randomDeviation != 0) {
			if(grabbedByHornets) {
				headRotationDeg += UnityEngine.Random.Range(-GRAB_ROTATION_RANDOM, GRAB_ROTATION_RANDOM);
			}
			else {
				headRotationDeg += randomDeviation * (Mathf.PerlinNoise (wavingPhase, 0) - 0.5f);
			}
		}
		
		#endregion
		#region "Head final application"
		
		// Make sure we don't go out of authorized speed
		targetSpeed = speedLimit.Clamp(targetSpeed);
		
		// Compute instant speed
		//speed = (targetSpeed - speed) * acceleration * Time.deltaTime;

		speed = Mathf.Lerp (speed, targetSpeed, acceleration * Time.deltaTime);
		if(diving)
		{
			speed *= seaCoef;
			targetSpeed = 0f;
		}
		speed = speedLimit.Clamp(speed);
		
		// Update head rotation from target angle
		lastFrameHeadRotationDeg = headRotationDeg; // memorize last rotation
		headRotationDeg = Mathf.LerpAngle (
			headRotationDeg, // Current
			targetHeadRotationDegNorm, // Target
			// The faster it flies, the faster it turns
			rotationAbility.Lerp(speedLimit.GetT(speed)) * Time.deltaTime
		);

		// Rotate and move forward
		transform.rotation = Quaternion.Euler (0f, 0f, headRotationDeg);
		transform.position += speed * transform.right * Time.deltaTime;
		
		#endregion
		#region "Paint splash"
		
		// Note from gamers point-of-view:
		// doing zig-zags shouldn't work because the angular speed 
		// slightly crosses 0, and then resets splashTriggerTime.
		// The only way is to keep a constant angular speed, so make circles.
		
		if(Mathf.Abs(AngularSpeed) >= SPLASH_TRIGGER_ANGSPEED_DEG)
		{
			if(Time.time - splashTriggerTime >= SPLASH_TRIGGER_DELAY_S && !HasMoonPaint)
			{
				DoPaintSplash();
				splashTriggerTime = Time.time;
				/*if(!Settings.trailerMode)
					CommonSounds.Instance.playSplash();*/ //michele
			}
		}
		else
		{
			splashTriggerTime = Time.time;
		}
		
		#endregion
		
		#endregion
		#region "Body update"
		
		// The body must follow the head.
		bones [0].pos = transform.position;
		
		// Slightly offset body start (Kandinsky's head is transluscent)
		bones [0].pos.x -= transform.right.x;
		bones [0].pos.y -= transform.right.y;
	
		// Update core line
		for (uint i = 1; i < bones.Length; ++i)
		{
			bones [i].Update ();
		}
		
		// Send positions to the line renderer
		for (int i = 0; i < currentBoneCount; ++i)
		{
			lineRenderer.SetPosition (i, bones[i].pos);
		}
		
		#endregion
		#region "Tail update"
		
		DrakeBone lastBone = bones [currentBoneCount - 1];
		
		tail.transform.position = lastBone.pos;
		tail.transform.rotation = Quaternion.Euler (90f, 0f, 0f);
		tail.transform.RotateAround (Vector3.forward, lastBone.angleRad + Mathf.PI);
		
		#endregion
		
		// Debug
		// Displays the barycenter of the dragon
		/*if(Settings.debugMode)
		{
			Vector3 b = GetBarycenter();
			TmpDebug.Instance.transform.position = b;
//			Debug.DrawLine(
//				new Vector3(b.x-3, b.y-3, transform.position.z), 
//				new Vector3(b.x-3, b.y-3, transform.position.z), Color.red, 0.5f);
		}*/	
	}
	
	/// <summary>
	/// Gets the growth ratio of the dragon in [0,1].
	/// 0 means the dragon didn't grew up,
	/// 1 means the dragon has been enlarged to maximum size.
	/// </summary>
	/// <returns>Value in [0, 1]</returns>
	public float GetGrowthRatio()
	{
		return (float)(currentBoneCount - startBoneCount) 
			/ (float)(BONE_COUNT_MAX - startBoneCount);
	}
	
	/// <summary>
	/// Discharges all the paint into a big splash.
	/// Does nothing if the color level is 0.
	/// </summary>
	private void DoPaintSplash()
	{
		if(!HasColor)
			return; // Can't do this, I have no paint !
		
		// Spawn the splash
		Vector3 splashPos = transform.position;//GetBarycenter();
		// It will increase during the level, when the dragon grows.
		float splashSizeMultiplier = 1f + GetGrowthRatio();
		
		PolyPaintManager.Instance.SpawnSplash(
			scales[1].ColorIndex, scales[1].Color, 
			splashPos.x, splashPos.y, 
			splashSizeMultiplier);
		
		// Clear paint level
		for(int i = 0; i < scales.Length; ++i)
		{
			scales[i].ColorLevel = 0;
		}
	}
	
	public float Speed // in world units / s
	{
		get { return speed; }
	}
	
	public float AngularSpeed // in degrees
	{
		get { 
			return Mathf.DeltaAngle(
				lastFrameHeadRotationDeg, 
				headRotationDeg) / Time.deltaTime; 
		}
	}
	
	/// <summary>
	/// Makes the drake bounce against a circular object.
	/// The radius is the distance of the given center to the drake.
	/// The bounce vector is computed from drake's motion vector and the center.
	/// </summary>
	/// <param name='center'>Center of the circular object.</param>
	void BounceAgainst(Vector2 center)
	{
		Vector2 pos = new Vector2(transform.position.x, transform.position.y);
		Vector2 u = center - pos;
		u.Normalize();
		Vector2 v = new Vector2(transform.right.x, transform.right.y);
		float dot = Vector2.Dot(u, v);
		if(dot > 0)
		{
			// Hard rotation
			Vector2 w = v - dot * u;
			targetHeadRotationDegNorm = Mathf.Rad2Deg * Mathf.Atan2(w.y, w.x);
			headRotationDeg = targetHeadRotationDegNorm;
			//Debug.Log("Bounce " + u + ", " + v + ", " + w);
			
			// Add speed
			speed *= 1.3f;
			
			// Shake camera
			CameraHandler ch = Camera.mainCamera.GetComponent<CameraHandler>();
			if(ch != null)
			{
				ch.Shake(3);
			}
		}
	}
	
	// Called when the drake is attacked by hornets
	public void GrabbedByHornet() 
	{
		grabbedByHornets = true;
	}
	
	// Called when hornets stop attacking the drake
	public void UngrabbedByHornet() 
	{
		grabbedByHornets = false;
	}
	
	void OnTriggerEnter (Collider other)
	{
		if (other.GetComponent<Cloud>() != null) 
		{
			OnCloudEnter ();
		}
		if(other.name == "PetrolBall")
		{
			BounceAgainst(new Vector2(other.transform.position.x, other.transform.position.y));
			Stun(STUN_DURATION);
			OnHitPetrol();
			SoundLevel2.Instance.HitPetrol();
		}
		if(other.name == "Moon")
		{
			CommonSounds.Instance.MoonEnter();
		}
		if(other.GetComponent<PowerableStar>() != null)
		{
			SoundLevel1.Instance.HitStar(other.gameObject);
		}
		if(other.GetComponent<SwappableStar>() != null)
		{
			SoundLevel2.Instance.HitStar(other.gameObject);// michèle changer
		}
	}
	
	void OnTriggerExit (Collider other)
	{
		Cloud cloud = other.GetComponent<Cloud>();
		if (cloud != null)
		{
			OnCloudExit (cloud);
		}
		if(other.name == "Moon")
		{
			CommonSounds.Instance.MoonExit();
		}
	}
	
	void OnCloudEnter()
	{
		//Debug.Log ("Cloud enter");
		switch(Level.Get.levelID)
		{
		case 2:
			SoundLevel2.Instance.CloudEnter();
			break;
		default:
			break;
		}
	}
	
	void OnCloudExit(Cloud cloud)
	{
		if(Level.Get.levelID == 2)
		{
			PolyPaintManager.Instance.SpawnCloudProjection(
				cloud.colorIndex, 
				transform.position.x, 
				transform.position.y, 
				headRotationDeg);
		}
		else
		{
			PolyPaintManager.Instance.SpawnCloudProjection(
				cloud.CurrentColor, 
				transform.position.x, 
				transform.position.y, 
				headRotationDeg);
		}
		switch(Level.Get.levelID)
		{
		case 1:
			SoundLevel1.Instance.CloudExit(cloud.gameObject);
			break;
		case 2:
			SoundLevel2.Instance.CloudExit();
			break;
		default:
			break;
		}
	}
	
	/// <summary>
	/// Called when the dragon dives down into the sea
	/// </summary>
	void OnSeaDive()
	{
		//Debug.Log("OnSeaDive");
	}
	
	/// <summary>
	/// Called when the dragon goes out of the sea from the above
	/// </summary>
	void OnSeaSurface()
	{
		//Debug.Log("OnSeaSurface");
	}
	
	void OnHitPetrol()
	{
	}
	
	/// <summary>
	/// Called when a part of the dragon touches a star
	/// </summary>
	/// <param name='starObj'>Star object.</param>
//	public void OnTouchedStar(GameObject starObj)
//	{
//		lastTouchedStar = starObj;
//	}
	
	public GameObject LastTouchedStar
	{
		get { return lastTouchedStar; }
		set
		{
			lastTouchedStar = value;
		}
	}
	
	/// <summary>
	/// Does the drake has some color paint?
	/// </summary>
	/// <value>
	/// <c>true</c> if yes otherwise, <c>false</c>.
	/// </value>
	public bool HasColor
	{
		get
		{
			for(int i = 0; i < currentScaleCount; ++i)
			{
				if(scales[i].HasColor)
					return true;
			}
			return false;
		}
	}
	
	public bool HasMoonPaint
	{
		get
		{
			for(int i = 0; i < currentScaleCount; ++i)
			{
				if(scales[i].HasMoonPaint)
					return true;
			}
			return false;
		}
	}
	
	public float FullColorLevel
	{
		get
		{
			float sum = 0;
			for(int i = 0; i < currentScaleCount; ++i)
			{
				sum += scales[i].ColorLevel;
			}
			return sum / (float)(currentScaleCount);
		}
	}
	
	void OnGUI()
	{
		if(Settings.debugMode)
		{
			GUI.Label(new Rect(16, 60, 512, 32), 
				"Drake speed : " + speed
				+ ", pos : " + transform.position
				+ ", angularSpeed : " + (int)AngularSpeed, 
				Settings.debugGuiStyle);
			
			GUI.Label(new Rect(16, 80, 256, 32), 
				"Wrapped altitude : " + Level.Get.GetWrappedAltitude(transform.position.y),
				Settings.debugGuiStyle);
			
			GUI.Label(new Rect(16, 100, 256, 32), 
				"UPS : " + (int)(1f / (Time.deltaTime == 0f ? 1 : Time.deltaTime)),
				Settings.debugGuiStyle);
		}
	}

}
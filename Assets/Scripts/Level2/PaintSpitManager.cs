using UnityEngine;
using System.Collections;

/// <summary>
/// This is a global pool of pre-instancied objects.
/// Use it to spawn PaintSpits or other dynamic objects.
/// </summary>
public class PaintSpitManager : MonoBehaviour
{
	private static PaintSpitManager globalInstance;
	
	public int poolSize = 64;
	
	public GameObject paintSpitPrefab;
	private GameObject[] paintSpitPool;
	
	void Awake()
	{
		globalInstance = this;
	}
	
	void Start ()
	{
		// Pre-build spit objects
		paintSpitPool = new GameObject[poolSize];
		for(uint i = 0; i < paintSpitPool.Length; i++)
		{
			GameObject obj = Instantiate(paintSpitPrefab) as GameObject;
			Helper.SetActive(obj, false);
			paintSpitPool[i] = obj;
		}
	}
	
	public static PaintSpitManager Instance
	{
		get { return globalInstance; }
	}
	
	private Vector3 tempPos, tempVel;
	public void SpawnPaintSpit(Vector3 pos, PaintSpitConfig cfg, sbyte channel)
	{
		// Search for an inactive object
		for(uint i = 0; i < paintSpitPool.Length; ++i)
		{
			GameObject obj = paintSpitPool[i];
			
			if(!Helper.IsActive(obj)) // Is this one inactive?
			{
				Helper.SetActive(obj, true); // Activate
				
				PaintSpit ps = obj.GetComponent<PaintSpit>();
				if(ps != null) // Should always work
				{
					float angleDeg = cfg.angleDeg;
					float speed = cfg.speed;
					//Color color = cfg.color;
					
					// A bit of random
					angleDeg += Random.Range(-8, 8);
					speed *= Random.Range(0.8f, 1.2f);
					//color.a = Mathf.Clamp(color.a * Random.Range(0.8f, 1.2f), 0, 1);
					
					tempPos.x = pos.x + cfg.offset.x;
					tempPos.y = pos.y + cfg.offset.y;
					tempPos.z = pos.z;
					
					tempVel.x = Mathf.Cos(Mathf.Deg2Rad*cfg.angleDeg) * speed;
					tempVel.y = Mathf.Sin(Mathf.Deg2Rad*cfg.angleDeg) * speed;
					
					// Another bit of random
					tempPos.x += Random.Range(-5, 5);
					tempPos.y += Random.Range(-5, 5);
					
					ps.Reset(tempPos, tempVel, channel);
				}
				else
					Debug.LogError("Pool: PaintSpit is not a PaintSpit !");
				
				break;
			}
		}
	}
	
	void OnGUI()
	{
		if(Settings.debugMode)
		{
			int n = 0;
			for(uint i = 0; i < paintSpitPool.Length; ++i)
			{
				if(paintSpitPool[i].active)
					++n;
			}
			int pc = (int)((float)n / (float)paintSpitPool.Length);
			GUI.Label(new Rect(16, 350, 256, 302), 
				"PaintSpit pool use : " + pc + " %",
				Settings.debugGuiStyle);
		}
	}

}




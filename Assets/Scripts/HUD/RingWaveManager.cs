using UnityEngine;
using System.Collections;

public class RingWaveManager : MonoBehaviour
{
	private const float SPAWN_INTERVAL = 0.25f;
	
	private GameObject[] waves;
	private Vector2 spawnPos;
	private int remaining;
	private float lastSpawnTime;
	
	void Start()
	{
		waves = new GameObject[4];
		for(int i = 0; i < waves.Length; ++i)
		{
			GameObject obj = new GameObject();
			obj.AddComponent<RingWave>();
			obj = Instantiate(obj) as GameObject;
			obj.transform.parent = this.gameObject.transform;
			//obj.transform.position = new Vector3(0,0,0);
			Helper.SetActive(obj, false);
			waves[i] = obj;
		}
	}
	
	public void SpawnSeries(float x, float y, int count)
	{
		if(remaining == 0)
		{
			spawnPos.x = x;
			spawnPos.y = y;
			remaining = count;
		}
	}
	
	private void Spawn()
	{
		for(int i = 0; i < waves.Length; ++i)
		{
			if(!Helper.IsActive(waves[i]))
			{
				waves[i].GetComponent<RingWave>().Spawn(spawnPos.x, spawnPos.y);
				break;
			}
		}
	}
	
	void Update()
	{
		if(remaining > 0)
		{
			if(Time.time - lastSpawnTime > SPAWN_INTERVAL)
			{
				Spawn();
				lastSpawnTime = Time.time;
				--remaining;
			}
		}
	}

}











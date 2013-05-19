using UnityEngine;
using System.Collections;

/// <summary>
/// Animates a particle system representing bugs on a game object
/// </summary>
public class BugBoid : MonoBehaviour 
{
	public int COUNT = 32;
	public float RADIUS = 16f;
	private ParticleSystem system;
	private ParticleSystem.Particle[] particles;

	void Start () 
	{
		system = GetComponent<ParticleSystem>();
		particles = new ParticleSystem.Particle[COUNT];
		for(int i = 0; i < particles.Length; ++i)
		{
			ParticleSystem.Particle p = new ParticleSystem.Particle();
			particles[i] = p;
		}
	}
	
	void Update () 
	{
		Vector3 center = transform.position;
		Vector3 pos;
	
		for(int i = 0; i < particles.Length; ++i)
		{
			pos = particles[i].position;

			pos.x = Random.Range(-RADIUS, RADIUS);
			pos.y = Random.Range(-RADIUS, RADIUS);
			
			pos.z = center.z;
			
			particles[i].position = pos;
			particles[i].lifetime = 999999999f;
		}
		
		system.SetParticles(particles, particles.Length);
	}

}

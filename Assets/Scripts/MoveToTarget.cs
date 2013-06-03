using UnityEngine;
using System.Collections;

/// <summary>
/// Simply displaces an object from its position to a given target once enabled.
/// </summary>
public class MoveToTarget : MonoBehaviour
{
	public Transform target;
	
	private const float DURATION = 15f;
	
	private Vector3 startPos;
	private float progress = -1;
	
	void Start ()
	{
		startPos = transform.position;
		if(target == null)
			Debug.LogError(name + ": I need a target !");
		
		StartMovement();
	}
	
	public void StartMovement()
	{
		progress = 0.001f;
		Level.Get.Detach(gameObject);
	}
	
	void Update ()
	{
		if(progress < 0)
			return;

		if(progress < 1f)
		{
			progress += Time.deltaTime * (1f / DURATION);
			
			Debug.Log(progress);
			if(progress > 1f || Vector3.Distance(
				transform.position, Camera.mainCamera.transform.position) > 5*Tile.SIZE)
			{
				progress = -1;
				Level.Get.Attach(gameObject);
			}
			
			transform.position = Vector3.Lerp(
				startPos, target.transform.position, progress);
		}
	}

}

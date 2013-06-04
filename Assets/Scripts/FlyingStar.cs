using UnityEngine;
using System.Collections;

public class FlyingStar : MonoBehaviour
{
	public Transform finalTarget;
	public GameObject objectToEnable;
	
	private const float SPEED = 50f;
	
	void Start()
	{
		MoverLevelWrapper mlw = GetComponent<MoverLevelWrapper>();
		if(mlw == null)
			gameObject.AddComponent<MoverLevelWrapper>();
		if(finalTarget == null)
			Debug.LogError(name + ": I need a final target !");
		if(objectToEnable == null)
			objectToEnable = finalTarget.gameObject;
		SoundLevel2.Instance.birthFlyingStar(this.gameObject);
	}
	
	void Update ()
	{
		Vector3 pos = transform.position;
		
		if(Level.Get.IsSpace(pos.x, pos.y))
		{
			// I'm in space !
			Vector3 targetPos = finalTarget.position;
			Vector2 v = new Vector2(targetPos.x-pos.x, targetPos.y-pos.y);
			v.Normalize();
			pos.x += v.x * SPEED * Time.deltaTime;
			pos.y += v.y * SPEED * Time.deltaTime;
			
			float sqrd = Helper.SqrDistance(targetPos.x, targetPos.y, pos.x, pos.y);
			
			// Am I close enough to my target?
			if(sqrd < 2f*2f)
			{
				// I finished my job, activate another object if specified
				if(objectToEnable != null)
				{
					Helper.SetActive(objectToEnable, true);
					SoundLevel2.Instance.registerStar(objectToEnable);
//					HaloBehaviour halo = objectToEnable.GetComponent<HaloBehaviour>();
//					if(halo != null)
//						halo.Blink();
				}
				// And deactivate myself
				Helper.SetActive(gameObject, false);
			}
		}
		else
		{
			// Go up anyway, I want to go in space. Not in the sea.
			pos.y += SPEED * Time.deltaTime;
		}
		
		transform.position = pos;
	}

}

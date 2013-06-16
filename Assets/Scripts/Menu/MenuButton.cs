using UnityEngine;
using System.Collections;

public class MenuButton : MonoBehaviour {
	
	
	private bool clicked = false;

	
	// Update is called once per frame
	void Update () 
	{
		bool pressing;
		if(Settings.onTablet) {
			pressing = Input.touchCount != 0; // Touch input
		} else {
			pressing = Input.GetMouseButton(0); // Mouse input (desktop tests)
		}
		
		if(pressing)
		{
			// Get touch position
			Vector2 touchPos = new Vector2();
			if(Settings.onTablet) {
				Touch t = Input.GetTouch(0);
				touchPos = t.position;
			} else  {
				touchPos = Input.mousePosition;
			}
			
			// Convert into scene scale
			Vector3 currentPos = ScreenToScenePosition(touchPos);
			Vector3 scale = transform.localScale;
			Vector3 position= transform.position;
			// Click in the object
			if(currentPos.x > position.x - scale.x/2 &&
				currentPos.x < position.x + scale.x/2 &&
				currentPos.y > position.y - scale.y/2 &&
				currentPos.y < position.y + scale.y/2)
			{
				clicked = true;
			}
		}
	}
	
	private Vector3 ScreenToScenePosition(Vector2 pos)
	{
		// We'll use a plane to convert touch to scene coordinates 
		// (should be at same depth as character's head)
		Plane plane = new Plane(-Vector3.forward, transform.position);
		
		// Raycast to the plane
		float hitDistance = 0f;
		Ray touchRay = Camera.main.ScreenPointToRay(pos);
		if(plane.Raycast(touchRay, out hitDistance)) // If we hit the plane (actually always)
		{
			Vector3 scenePos = touchRay.GetPoint(hitDistance);
			//if(reference != null)
				//scenePos -= reference.position;
			return scenePos;
		}
		else
			return new Vector3(0,0,0); // Should never happen
	}
	
	public bool IsClicked()
	{
		return clicked;
	}
	
	public void setClicked(bool click)
	{
		clicked = click;
	}
	
}

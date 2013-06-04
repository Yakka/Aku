using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Converts touch patterns into actions the drake has to do.
/// </summary>
public class TouchController : MonoBehaviour 
{	
	//===== Config =====
	
	public Drake drakeRef; // The controlled object
	public Transform reference; // Usually the camera	
	//public Range tapDuration; // time span assumed for a brief static touch with the finger
	//public float minDragLength; // Dragging is not relevant under this threshold, and will be interpreted as a tap
	//public float minInstantSpeed; // Under this speed, dragging is not relevant
	//public float minPointTime; // Finger pointing minimal time, when not dragging
	
	//===== Dynamic vars =====
	
	private float beginTime; // Touch down
	private Vector3 beginPos;
	
	private float endTime; // Touch release
	private Vector3 endPos; // Note : equivalent to current touch pos as it is updated at each frame
	
	private Vector3 lastFramePos;
	
	private bool pressing; // Indicates that the player touches the screen
	private bool everPressed;
	
	//private bool pointed; // Indicates that OnPoint() has been called for the current touch
	
	//===== Debug =====
	public bool displayTrace;
	private LineRenderer lineRenderer; // Debug

	//============== Methods ================
	
	void Start () 
	{
	}
	
	void Update () 
	{	
		// Detect when we are touching the screen
		bool wasPressed = pressing;
		if(Settings.onTablet) {
			pressing = Input.touchCount != 0; // Touch input
		} else {
			pressing = Input.GetMouseButton(0); // Mouse input (desktop tests)
		}
		
		// If we are touching the screen
		if(pressing)
		{
			everPressed = true;
			
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
			
			if(!wasPressed) // Are we starting a new touch?
			{
				// Memorize touch start
				beginTime = Time.time;
				beginPos = currentPos;
				lastFramePos = beginPos;
				OnTouchDown();
			}
			else
				lastFramePos = endPos;
			endPos = currentPos; // The end pos is always the last captured
			endTime = Time.time;
			
			OnTouchMove();
			
			// If the touch moved with a relevant speed
//			if(Length >= minDragLength && GetInstantSpeed() >= minInstantSpeed)
//			{
//				OnLead(); // It's a drag
//			}
//			else if(Duration > minPointTime)
//			{
//				if(!pointed)
//				{
//					OnPoint(); // It's a pointing
//					pointed = true; // Avoid calling OnPoint() each frame after this one
//				}
//			}
		}
		else if(wasPressed) // Did the move just finished?
		{
			OnTouchUp();
			//Debug.Log("Touch finished (duration=" + Duration + ", length=" + Length);
			//pointed = false;
			// If it wasn't a drag and was brief
//			if(Length < minDragLength && tapDuration.Contains(Duration))
//			{
//				OnTap(); // It's a tap
//			}
			//endTime = Time.time;
		}
	
	#region "Debug"
		// Draws the touch on the screen
		if(Settings.debugMode && displayTrace)
		{
			if(lineRenderer == null)
			{
				lineRenderer = gameObject.AddComponent<LineRenderer>();
				lineRenderer.material = new Material(Shader.Find("Particles/Alpha Blended"));
				lineRenderer.SetColors(Color.black, Color.cyan);
				lineRenderer.SetWidth(0.2F, 0.2F);
				lineRenderer.SetVertexCount(2);
			}
			
			Vector3 a = beginPos;
			Vector3 b = endPos;
			
//			float angle = GetInstantAngleRad();
//			Vector3 b = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
//			b = a + b * (0.1f * GetInstantSpeed() + 5f);
			
			lineRenderer.SetPosition(0, a);
			lineRenderer.SetPosition(1, b);
		}
	#endregion
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
	
	private void OnTouchDown()
	{
		drakeRef.OrientateTo(beginPos);
		drakeRef.Dash();
	}
	
	private void OnTouchMove()
	{
		drakeRef.OrientateTo(endPos);
	}
	
	private void OnTouchUp()
	{
	}
	
	/// <summary>
	/// Called when we assume that the player just tapped the screen
	/// </summary>
	/*private void OnTap()
	{
		//Debug.Log("Tap (" + Duration + ")");
		drakeRef.OrientateTo(beginPos + reference.position);
	}*/
	
	/// <summary>
	/// Called when the player puts its finger on the screen without moving it
	/// </summary>
	/*private void OnPoint()
	{
		//Debug.Log("Point");
	}*/

	/// <summary>
	/// Called while the player is dragging its finger
	/// </summary>
	/*private void OnLead()
	{
		drakeRef.Lead(this);
	}*/
	
	/// <summary>
	/// World position where the touch started.
	/// Relative to reference.
	/// </summary>
	public Vector3 Begin
	{
		get { return beginPos; }
	}
	
	/// <summary>
	/// World position where the touch ended or currently is.
	/// Relative to reference.
	/// </summary>
	public Vector3 End
	{
		get { return endPos; }
	}
		
	public float Length
	{
		get { return (End - Begin).magnitude; }
	}
	
	/// <summary>
	/// Returns the time elapsed between touch begin and current touch.
	/// If there is no touch, returns the last touch duration.
	/// </summary>
	public float Duration
	{
		get { return endTime - beginTime; }
	}
	
	/// <summary>
	/// Computes the instant touch speed in unit/s.
	/// Returns 0 if there is no touch.
	/// </summary>
	public float GetInstantSpeed()
	{
		if(!pressing)
			return 0;
		return (lastFramePos - endPos).magnitude / Time.deltaTime;
	}
	
	public float GetInstantAngleRad()
	{
		float dx = endPos.x - lastFramePos.x;
		float dy = endPos.y - lastFramePos.y;
		float t = Mathf.Atan2(dy, dx);
		return t;
	}
	
	/// <summary>
	/// Indicates if the player is absolutely touching the screen.
	/// </summary>
	public bool IsPressing()
	{
		return pressing;
	}
	
	public float EndTime
	{
		get { return endTime; }
	}
	
	public float BeginTime
	{
		get { return beginTime; }
	}
	
	public bool EverPressed
	{
		get { return everPressed; }
	}
	
}




using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PaintScreen : MonoBehaviour
{
	private bool isFirstUpdate = true;
	
//	void Start()
//	{
//	}
	
	void Update () 
	{
		if(isFirstUpdate) // TODO This is not clean, but didn't worked in Start()
		{
//			Shader shader = Shader.Find("Mobile/Particles/Alpha Blended");
//			if(shader == null)
//				Debug.Log(name + ": Shader not found");
//			Material mat = new Material(shader);
			
			//Material mat = PolyPaintManager.Instance.PixelPaintMaterial;
			Material mat = new Material(Helper.FindShader("Mobile/Particles/Alpha Blended"));
			renderer.material = mat;
			renderer.material.mainTexture = PaintCameraHandler.Instance.renderTarget;
			
			//Debug.Log("aaa");
			isFirstUpdate = false;
		}
		
		Vector3 pos = Camera.mainCamera.transform.position;
		pos.z = 5;
		transform.position = pos;
		float s = 2.01f*Camera.mainCamera.orthographicSize;
		float ratio = Camera.mainCamera.aspect;
		//Debug.Log(s);
		transform.localScale = new Vector3(s*ratio, s, 1);
	}

}



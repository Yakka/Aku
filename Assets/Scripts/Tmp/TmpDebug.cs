using UnityEngine;
using System.Collections;

public class TmpDebug : MonoBehaviour 
{
	private static TmpDebug globalInstance;
	//public int newStripsPerFrame;
	
	void Awake()
	{
		globalInstance = this;
	}
	
	public static TmpDebug Instance
	{
		get { return globalInstance; }
	}
	
	void Update()
	{
		//newStripsPerFrame = 0;
	}

}

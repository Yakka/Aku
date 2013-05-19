using UnityEngine;
using System.Collections;

public class TmpDebug : MonoBehaviour 
{
	private static TmpDebug globalInstance;
	
	void Awake()
	{
		globalInstance = this;
	}
	
	public static TmpDebug Instance
	{
		get { return globalInstance; }
	}

}

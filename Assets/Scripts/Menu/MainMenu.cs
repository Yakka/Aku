using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
	
	public MenuButton[] buttons;
	
	public string firstScene;
	
	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
		for(int i = 0; i < buttons.Length; i++)
		{
			if(buttons[i].IsClicked())
			{
				switch(buttons[i].name)
				{
				case "Exit Button":
						Application.Quit();
					break;
				case "Play Button":
					Application.LoadLevel(firstScene);
					break;
				case "Credits Button":
					break;
				}
			}
		}
		
	}
}

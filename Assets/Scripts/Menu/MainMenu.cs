using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
	
	public MenuButton[] buttons;
	public Travelling travelling;
	public string firstScene;
	public string creditsScene;
	public string menuScene;
	public string loadingScene;
	public GameObject[] exitConfirmation;
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
				case "No":
					for(int j = 0; j < exitConfirmation.Length; j++)
						Helper.SetActive(exitConfirmation[j], false);
					break;
				case "Yes":
					Application.Quit();
					break;
				case "Exit Button":
					for(int j = 0; j < exitConfirmation.Length; j++)
						Helper.SetActive(exitConfirmation[j], true);
					break;
				
				case "Credits Button":
					Application.LoadLevel(creditsScene);
					break;
					
				case "Play Button":
					Application.LoadLevel(loadingScene);
					break;
				case "Back Button":
					Application.LoadLevel(menuScene);
					break;
				case "Next Button":
					Application.LoadLevel(firstScene);
					break;
				}
				buttons[i].setClicked(false);
			}
		}
		
	}
}

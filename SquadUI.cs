using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquadUI : MonoBehaviour
{
    //UI components
	Text toggle_text;
	bool if_show_squad_info = false;
	RectTransform squad_ui;
	GameObject ui_panel;

	//unit related
	GameObject[] units;


	// Start is called before the first frame update
	void Start()
    {
		//text change
		toggle_text = transform.Find("toggleButton").Find("Text").GetComponent<Text>();

		squad_ui = transform.Find("squadUI").Find("Panel").GetComponent<RectTransform>();
		ui_panel = GameObject.Find("Panel");
		ui_panel.SetActive(false);

		//find all units object
		units = GameObject.FindGameObjectsWithTag("Player");
	}


    //handle toggling to show/hide UI 
    public void buttonClicked()
	{
		if_show_squad_info = !if_show_squad_info;
		if (if_show_squad_info)
		{
			toggle_text.text = "<";
			ui_panel.SetActive(true);
		}
		else
		{
			ui_panel.SetActive(false);
			toggle_text.text = ">";
			
		}
	}

}

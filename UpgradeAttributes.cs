using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UpgradeAttributes : MonoBehaviour
{
    public string attributeType;
    public int unit_num;
    Button btn;
    GameObject playerUnits;
    
    GameObject[] units;
    GameObject unit;
    float currency;
    
    void Start(){
        //find all units object
		units = GameObject.FindGameObjectsWithTag("Player");
        unit = units[unit_num-1];
        
        btn = transform.Find("Button").GetComponent<Button>();
        playerUnits = GameObject.Find("PlayerUnits");
//        Debug.Log( playerUnits.GetComponent<PlayerManager>().getCurrency());
    }
    
    void Update(){
        currency = playerUnits.GetComponent<PlayerManager>().getCurrency();
//        Debug.Log(currency);
        if(currency <= 20){
            btn.interactable = false;
        }else{
            btn.interactable = true;
        }
    }
    
    //handle toggling to show/hide UI 
    public void buttonClicked()
	{
		unit.GetComponent<PlayerController>().updateStatus(attributeType);
        playerUnits.GetComponent<PlayerManager>().updateCurrency(15);
    }
}

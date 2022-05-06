using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> myUnits;
    public Text currencyText;
    public float currency = 0f;
    public float currencyPerTurn = 100f;
    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform child in transform)
        {
            myUnits.Add(child.gameObject);
        }   
    }

    // Update is called once per frame
    void Update()
    {
        currencyText.text = "" + currency;
    }

    public void ResetMe()
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<unitRoundResetLocal>().ResetMe();
        }
    }

    public void AddCurrency()
    {
        currency += currencyPerTurn;
    }
    
    public float getCurrency(){
        return currency;
    }
    
    public void updateCurrency(float cost){
        currency -= cost;
    }
}

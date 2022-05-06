using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour
{
    GameObject menuButton;
    bool menuActive = false;

    // Start is called before the first frame update
    void Start()
    {
        menuButton = transform.GetChild(0).gameObject;
        menuButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuActive)
            {
                menuButton.SetActive(false);
                menuActive = false;
            }
            else if (!menuActive)
            {
                menuButton.SetActive(true);
                menuActive = true;
            }
        }   
    }
}

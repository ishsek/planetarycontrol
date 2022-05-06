using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class cameraLocScript : MonoBehaviour
{
    public GameObject myAttachedCam;
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (myAttachedCam.GetComponent<isometricCamera>().enabled == false)
        {
            myAttachedCam.GetComponent<isometricCamera>().enabled = true;
        }
        if (myAttachedCam.GetComponent<ThirdPersonCamera>().enabled == false)
        {
            myAttachedCam.GetComponent<ThirdPersonCamera>().enabled = true;
        }
    }
}

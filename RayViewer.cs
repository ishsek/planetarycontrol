using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RayViewer : MonoBehaviour {
    private gameModeManager modeManager;
    public float weaponRange = 50f;
    public Camera tpCam;

	// Use this for initialization
	void Start () {
        modeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameModeManager>();
    }
	
	// Update is called once per frame
	void Update () {
        //if (hasAuthority)
        //{
        if (modeManager == null) { modeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameModeManager>(); return; }

        if (modeManager.currentMode == gameModeManager.Mode.thirdperson)
        {
            if (tpCam == null) tpCam = Camera.main;
            Vector3 lineOrigin = tpCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            Debug.DrawRay(lineOrigin, tpCam.transform.forward * weaponRange, Color.green);
            //Debug.Log("draw my debug green ray");
        }
        else
        {

        }
        //}
	}
}

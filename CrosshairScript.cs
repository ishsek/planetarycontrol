using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrosshairScript : MonoBehaviour
{
    gameModeManager modeManager;
    // Start is called before the first frame update
    void Start()
    {
        modeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameModeManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (modeManager == null) modeManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<gameModeManager>();
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Level2")) return;
        if (gameObject.activeSelf && modeManager.currentMode != gameModeManager.Mode.thirdperson) gameObject.SetActive(false);
    }
}

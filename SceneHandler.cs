using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public bool sceneloaded = false;
    public bool cameraSet = false;
    private AsyncOperation sceneAsync;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (sceneloaded == false)
        {
            GetComponent<NetworkManagerHUD>().showGUI = true;
            GameObject[] playersReady = GameObject.FindGameObjectsWithTag("PlayerNetworkObject");
            if (playersReady.Length > 0)
            {
                StartCoroutine(loadScene("Level2"));
                //scene = SceneManager.LoadSceneAsync("Level2");
                //scene.allowSceneActivation = false;
                //sceneloaded = true;
                sceneloaded = true;
            }
        }
        else
        {
            //if (scenemanager.getactivescene() != scenemanager.getscenebyname("level2"))
            //{
            //    scenemanager.setactivescene(scenemanager.getscenebyname("level2"));
            //}
            if (cameraSet == false)
            {
                Camera.main.transform.parent.SetParent(GameObject.FindGameObjectWithTag("SceneChangeUtilityParent").transform);
                cameraSet = true;
            }

        }
    }

    IEnumerator loadScene(string scenename)
    {
        AsyncOperation scene = SceneManager.LoadSceneAsync(scenename, LoadSceneMode.Additive);
        scene.allowSceneActivation = false;
        sceneAsync = scene;

        //Wait until we are done loading the scene
        while (scene.progress < 0.9f)
        {
            Debug.Log("Loading scene " + " [][] Progress: " + scene.progress);
            yield return null;
        }
        OnFinishedLoadingAllScene();
    }

    void enableScene(string scenename)
    {
        //Activate the Scene
        sceneAsync.allowSceneActivation = true;


        Scene sceneToLoad = SceneManager.GetSceneByName(scenename);
        if (sceneToLoad.IsValid())
        {
            Debug.Log("Scene is Valid");
            GameObject[] toMove = GameObject.FindGameObjectsWithTag("PlayerNetworkObject");
            Debug.Log("Number of players in scenehandler: " + toMove.Length);
            foreach (GameObject player in toMove)
            { 
                //SceneManager.MoveGameObjectToScene(player, sceneToLoad);
            }
            SceneManager.SetActiveScene(sceneToLoad);
        }
    }

    void OnFinishedLoadingAllScene()
    {
        Debug.Log("Done Loading Scene");
        enableScene("Level2");
        Debug.Log("Scene Activated!");
    }

    public static void LoadSceneAsync(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }
}

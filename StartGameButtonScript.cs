using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class StartGameButtonScript : MonoBehaviour
{
    public void StartMenu()
    {
        DestroyManager();
        SceneManager.LoadScene("MainMenu");
    }

    public void StartTutorial()
    {
        DestroyManager();
        SceneManager.LoadScene("TutorialScene");
    }

    public void StartGame()
    {
        DestroyManager();
        SceneManager.LoadScene("WorldMapTest");
    }

    public void StartLevel2()
    {
        //DestroyManager();
        SceneManager.LoadScene("Level2");
    }

    public void QuitGame()
    {
        DestroyManager();
        Application.Quit();
    }

    public void DestroyManager()
    {
        GameObject networkManagerObject = GameObject.Find("NetworkManager");
        if (networkManagerObject != null)
        {
            Destroy(networkManagerObject);
            NetworkManager.Shutdown();
        }
    }
}

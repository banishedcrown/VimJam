using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame ()
    {
        Debug.Log("Playing Game");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void MainMenuBack()
    {
        Debug.Log("GoingBack");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 2);
    }

    public void WebsiteURL()
    {
        Debug.Log("URL PROC");
        Application.OpenURL("https://www.banishedcrown.com");
    }
    public void QuitGame ()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class doorController : MonoBehaviour
{
    public bool needsCompletion = false;
    public string newScene;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("Door trigger entered!");

        if(!needsCompletion)
            SceneManager.LoadScene(newScene);

        else
        {
            //do nothing right now.
        }
    }
}

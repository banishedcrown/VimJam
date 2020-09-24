using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class doorController : MonoBehaviour
{
    public string newScene;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("Door trigger entered!");
        SceneManager.LoadScene(newScene);
    }
}

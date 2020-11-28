using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class doorControllerOpenable: MonoBehaviour
{
    public bool open = false;
    public string newScene;

    GameProgressTracker manager;

    private void Start()
    {
        GameObject m = GameObject.FindGameObjectWithTag("Manager");
        if (m != null) {
            manager = m.GetComponent<GameProgressTracker>();
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!needsCompletion)
        {
            if (manager != null)
            {
                if (!manager.canReturn)
                    SceneManager.LoadScene(newScene);
            }
            else
            {
                SceneManager.LoadScene(newScene);
            }
        }

        else
        {
            //do nothing right now.
            if (manager != null)
            {
                if (manager.canReturn)
                {
                    SceneManager.LoadScene(newScene);
                }
            }
        }
    }
}

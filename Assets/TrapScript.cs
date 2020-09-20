using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrapScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("Trap triggered!");
        if (collision.gameObject.tag == "Player")
        {
            trapTriggered(collision);
        }
    }

    void trapTriggered(Collider2D collision)
    {
        GameObject.Destroy(collision.gameObject);
        print("Game Over. Reloading scene.");

        //Reload the scene / respawn player
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

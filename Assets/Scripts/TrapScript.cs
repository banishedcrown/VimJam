using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrapScript : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject trapFront,trapBack;
    public Sprite inactiveState, activeState;

    private SpriteRenderer s_renderer;

    void Start()
    {
        s_renderer = GetComponent<SpriteRenderer>();
        s_renderer.sprite = inactiveState;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("Trap triggered!");
        if (collision.gameObject.tag == "Foot")
        {
            trapTriggered(collision);
        }
    }

    void trapTriggered(Collider2D collision)
    {

        s_renderer.sprite = activeState;

        collision.gameObject.transform.parent.gameObject.SendMessage("caughtPlayer");

        print("Game Over. Reloading scene.");

        //Reload the scene / respawn player
        Invoke("reloadScene", 2f);
    }

    void reloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

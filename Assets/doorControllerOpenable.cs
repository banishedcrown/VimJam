using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))] 
public class doorControllerOpenable: MonoBehaviour
{
    public bool open = false;
    public bool locked = true;
    public string newScene;

    public Sprite closedSprite;
    public Sprite openSprite;

    GameProgressTracker manager;
    private Animator animator;
    SpriteRenderer m_sprite;

    private void Start()
    {
        GameObject m = GameObject.FindGameObjectWithTag("Manager");
        if (m != null) {
            manager = m.GetComponent<GameProgressTracker>();
        }
        animator = GetComponent<Animator>();
        m_sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (open) { m_sprite.sprite = openSprite; }
        else { m_sprite.sprite = closedSprite; }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        print(collision.gameObject.name);
        if (collision.name == "FootTrigger")
        {
            if (!locked)
            {
                if (open)
                {
                    //SceneManager.LoadScene(newScene);
                }
                else
                {
                    m_sprite.sprite = openSprite;
                    open = true;
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name == "FootTrigger")
        {
            if (!locked)
            {
                if (open)
                {
                    m_sprite.sprite = closedSprite;

                    SceneManager.LoadScene(newScene);
                }
                else
                {
                    m_sprite.sprite = openSprite;
                    open = true;
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
}

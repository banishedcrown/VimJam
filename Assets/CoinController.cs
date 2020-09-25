using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    QuickTimer timer;
    public float fadeDelay = 5.0f;

    

    private void Start()
    {
        timer = new QuickTimer();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject player = GameObject.Find("Player");
        
        if(enemies.Length != 0)
            foreach (GameObject e in enemies)
            {
                e.SendMessage("CoinDropped", gameObject.transform);
            }

        player.SendMessage("CoinDropped", gameObject.transform);
    }
    private void Update()
    {
        if(timer.Elapsed() > fadeDelay)
        {
            GameObject.Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("coin collider");
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag=="Enemy")
        {
            GameObject.Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject player = GameObject.Find("Player");

        if (enemies.Length != 0)
            foreach (GameObject e in enemies)
            {
                e.SendMessage("CoinDestroyed", gameObject.transform);
            }

        player.SendMessage("CoinDestroyed", gameObject.transform);
    }

}

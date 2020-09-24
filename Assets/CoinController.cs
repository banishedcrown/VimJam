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
        if (collision.gameObject.tag == "Player")
        {
            GameObject.Destroy(gameObject);
        }
    }
}

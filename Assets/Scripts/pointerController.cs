using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointerController : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            GameObject.Destroy(gameObject);
        }
    }

    private void Start()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject player = GameObject.Find("Player");
        foreach(GameObject e in enemies)
        {
            e.SendMessage("CoinDropped", gameObject.transform);
        }

        player.SendMessage("CoinDropped", gameObject.transform);
    }

    private void OnDestroy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject player = GameObject.Find("Player");
        foreach (GameObject e in enemies)
        {
            e.SendMessage("CoinDestroyed", gameObject.transform);
        }

        player.SendMessage("CoinDestroyed", gameObject.transform);
    }

}

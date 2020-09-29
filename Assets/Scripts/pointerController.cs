using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointerController : MonoBehaviour
{
    [SerializeField] bool playerCoin = true;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && playerCoin)
        {
            GameObject.Destroy(gameObject);
        }
        else
        {
            if(collision.gameObject.tag == "Enemy" && !playerCoin)
            {
                GameObject.Destroy(gameObject);
            }
        }
    }

    private void Start()
    {

        if (playerCoin)
        {
            GameObject player = GameObject.Find("Player");
            player.SendMessage("CoinDropped", gameObject.transform);
        }
        else
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject e in enemies)
            {
                e.SendMessage("CoinDropped", gameObject.transform);
            }
        }
    }

    private void OnDestroy()
    {
        if (playerCoin)
        {
            GameObject player = GameObject.Find("Player");
            player.SendMessage("CoinDestroyed", gameObject.transform);
        }
        else
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject e in enemies)
            {
                e.SendMessage("CoinDestroyed", gameObject.transform);
            }
        }
    }

}

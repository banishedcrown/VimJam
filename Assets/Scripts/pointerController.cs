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
        GameObject player = GameObject.Find("Player");
        SpriteRenderer s_rend = GetComponent<SpriteRenderer>();
        s_rend.sortingOrder = transform.position.y > player.transform.position.y ? player.GetComponent<SpriteRenderer>().sortingOrder - 1 : player.GetComponent<SpriteRenderer>().sortingOrder + 1;
        if (playerCoin)
        {
            
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

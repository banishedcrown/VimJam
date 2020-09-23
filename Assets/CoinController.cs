using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        print("coin collider");
        if (collision.gameObject.tag == "Player")
        {
            GameObject.Destroy(gameObject);
        }
    }
}

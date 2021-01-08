using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairController : MonoBehaviour
{
    public float reductionPercent = 0.25f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Foot"))
        {
            collision.transform.parent.SendMessage("ChangeSpeed", 1 - reductionPercent);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Foot"))
        {
            collision.transform.parent.SendMessage("ChangeSpeed", 1);
        }
    }
}

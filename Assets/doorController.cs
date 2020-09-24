using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doorController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("Door trigger entered!");
    }
}

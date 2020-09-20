using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("trigger entered!");
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.SendMessage("RemoveGoal", gameObject);
        }
    }
}

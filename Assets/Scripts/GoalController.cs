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

    [SerializeField]
    float MaxDeflection=1, deflectionSpeed = 1;

    Vector2 startPosition;

    float currentDeflection = 0.0f;
    bool reversed = false;

    GameProgressTracker gameManager;

    private void Start()
    {
        startPosition = transform.position;

        GameObject gm = GameObject.FindGameObjectWithTag("Manager");
        gameManager = gm != null ? gm.GetComponent<GameProgressTracker>() : null;

        if (gameManager != null) if (gameManager.canReturn)
        {
            gameObject.active = false;
            return;
        }
    }
    private void FixedUpdate()
    {
        Vector2 pos = transform.position;
        pos.y = startPosition.y + currentDeflection;
        transform.position = pos;

        if (!reversed)
            currentDeflection += Time.fixedDeltaTime * deflectionSpeed;
        else currentDeflection -= Time.fixedDeltaTime * deflectionSpeed;

        if (currentDeflection >= MaxDeflection) reversed = true;
        if (currentDeflection <= 0) reversed = false;
    }
}

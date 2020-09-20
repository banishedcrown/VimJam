using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum EnemyState { IDLE, WANDER, PERSUE };
    private EnemyState state;
    private float timerStart;
    private float idleTime = 3; //3s
    private float wanderTime = 8; //8s

    // Start is called before the first frame update
    void Start()
    {
        timerStart = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        //Patrol, or wander
        switch (state)
        {
            case EnemyState.IDLE:
                break;
            case EnemyState.WANDER:
                //transform.children
                break;
            case EnemyState.PERSUE:
                break;
            default: break;
        }
    }

    private void FixedUpdate()
    {
        var timeElapsed = Time.time - timerStart;
        if (state == EnemyState.PERSUE && canSeePlayer())
        {
            return;
        }
        else
        {
            state = EnemyState.IDLE;
            timerStart = Time.time;
        }

        if (state == EnemyState.IDLE && timeElapsed > idleTime)
        {
            state = EnemyState.WANDER;
            timerStart = Time.time;
        }
        else if (state == EnemyState.WANDER && timeElapsed > wanderTime)
        {
            state = EnemyState.IDLE;
            timerStart = Time.time;
        }
        //Switch states here
    }

    private bool canSeePlayer()
    {
        return false;
    }
}

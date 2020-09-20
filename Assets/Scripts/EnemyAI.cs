using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum EnemyState { IDLE, WANDER, PERSUE };
    private EnemyState state;
    private QuickTimer timer;
    private float idleTime = 3; //3s
    private float wanderTime = 8; //8s

    // Start is called before the first frame update
    void Start()
    {
        timer = new QuickTimer();
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
                break;
            case EnemyState.PERSUE:
                break;
            default: break;
        }
    }

    private void FixedUpdate()
    {
        if(state == EnemyState.PERSUE && canSeePlayer())
        {
            return;
        }
        else
        {
            state = EnemyState.IDLE;
            timer.Reset();
        }
        if(state == EnemyState.IDLE && timer.elapsed > idleTime)
        {
            state = EnemyState.WANDER;
            timer.Reset();
        }
        else if(state == EnemyState.WANDER && timer.elapsed > wanderTime)
        {
            state = EnemyState.IDLE;
            timer.Reset();
        }
        //Switch states here
    }

    private bool canSeePlayer()
    {

        return false;
    }

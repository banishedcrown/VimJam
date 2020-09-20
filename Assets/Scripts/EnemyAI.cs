using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    enum EnemyState { IDLE, WANDER, PERSUE };
    EnemyState state;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Patrol, or wander
        switch(state)
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
        //Switch states here
    }



    
}

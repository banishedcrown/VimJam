﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerAI : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject pointer;
    private bool spawnedPointer = false;

    [Range(1, 10)]
    public float hearingRange = 4, seeingRange = 10;

    NavMeshAgent2D m_nav;
    List<GameObject> goals;

    enum PlayerStates { IDLE, SNEAKING, DISTRACTED, CAUGHT};
    PlayerStates state;

    QuickTimer idleTimer;
    private float maxIdleDelay = 3f;

    void Start()
    {
        m_nav = gameObject.GetComponent<NavMeshAgent2D>();

        foreach(GameObject g in GameObject.FindGameObjectsWithTag("Treasure"))
        {
            goals.Add(g);
        }
        ChangePlayerState(PlayerStates.IDLE);
        idleTimer = new QuickTimer;
    }

    GameObject minDest;
    float minDistance = float.PositiveInfinity;

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case PlayerStates.IDLE:
                m_nav.destination = gameObject.transform.position;

                if (idleTimer.Elapsed() > maxIdleDelay)
                {
                    ChangePlayerState(PlayerStates.DISTRACTED);
                }

                break;
            case PlayerStates.SNEAKING:
                
                if(Vector2.Distance(transform.position, m_nav.destination) < m_nav.stoppingDistance)
                {
                    ChangePlayerState(PlayerStates.IDLE);
                }
                break;
            case PlayerStates.DISTRACTED:
                
                foreach(GameObject g in goals)
                {
                    float d;
                    if (( d = Vector2.Distance(transform.position, g.transform.position)) < minDistance)
                    {
                        minDistance = d;
                        minDest = g;
                    }
                        
                }
                m_nav.destination = minDest.transform.position;

                break;
            case PlayerStates.CAUGHT:

                m_nav.destination = gameObject.transform.position;
                break;
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 w = Camera.main.ScreenToWorldPoint(Input.mousePosition);


            if (!spawnedPointer)
            {
                w.z = 0;
                GameObject.Instantiate(pointer, w, Quaternion.identity);
                spawnedPointer = true;
            }

            if (state != PlayerStates.CAUGHT && Vector2.Distance(w, transform.position) < hearingRange)
            {
                GetComponent<NavMeshAgent2D>().destination = w;
                ChangePlayerState(PlayerStates.SNEAKING);
            }
        }
        else
        {
            spawnedPointer = false;
        }
    }


    private void ChangePlayerState(PlayerStates newState)
    {
        switch (newState)
        {
            case PlayerStates.IDLE:
                state = PlayerStates.IDLE;
                idleTimer.Reset();
                maxIdleDelay = Random.Range(2f, 5f);
                break;
        }
    }

}

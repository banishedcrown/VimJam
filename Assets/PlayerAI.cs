using System.Collections;
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
    GameObject[] goals;

    enum PlayerStates { IDLE, SNEAKING, DISTRACTED, CAUGHT};
    PlayerStates state;

    QuickTimer idleTimer;
    
    void Start()
    {
        m_nav = gameObject.GetComponent<NavMeshAgent2D>();

        goals = GameObject.FindGameObjectsWithTag("Treasure");
        changePlayerState(PlayerStates.IDLE);
        idleTimer.Start();
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case PlayerStates.IDLE:
                m_nav.destination = gameObject.transform.position;

                break;
            case PlayerStates.SNEAKING:
                
                if(Vector2.Distance(transform.position, m_nav.destination) < m_nav.stoppingDistance)
                {
                    changePlayerState(PlayerStates.IDLE);
                }
                break;
            case PlayerStates.DISTRACTED:

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
                changePlayerState(PlayerStates.SNEAKING);
            }
        }
        else
        {
            spawnedPointer = false;
        }
    }


    private void changePlayerState(PlayerStates newState)
    {
        switch (newState)
        {
            case PlayerStates.IDLE:
                state = PlayerStates.IDLE;
                break;
        }
    }

}

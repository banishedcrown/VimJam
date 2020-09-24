using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]

public class EnemyAI : MonoBehaviour
{
    NavMeshAgent2D m_nav;
    [SerializeField]
    List<GameObject> goals;

    private enum EnemyState { IDLE, WANDER, PERSUE };
    private EnemyState state;
    private QuickTimer timer;
    private float maxIdleDelay;
    private float maxWanderTime = 8; //8s

    private float maxHearingDistance = 2f;

    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        timer = new QuickTimer();
        maxIdleDelay = Random.Range(0.5f, 5f);
        m_nav = gameObject.GetComponent<NavMeshAgent2D>();

        //Initialize player reference
        var players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 1) { print("Error, more than one Player!"); }
        else if (players.Length < 1) { print("Error, no Player!"); }
        else { player = players[0]; }

        //Initialize goals if necessary
        if(goals.Count == 0 || goals[0]==null)
        {
            var goalList = GameObject.FindGameObjectsWithTag("EnemyWaypoint");
            goals = new List<GameObject>(goalList);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Enemy in state " + state.ToString() + "and Player position is "+player.transform.position.ToString());
        switch (state)
        {
            case EnemyState.IDLE:
                if (canSeePlayer())
                {
                    setStatePersue();
                }
                else if (timer.Elapsed() > maxIdleDelay) //todo: make the max random
                {
                    setStateWander();
                }
                //else Leave destination set to self and continue
                break;
            case EnemyState.WANDER:
                if(canSeePlayer())
                {
                    setStatePersue();
                }
                //If we've reached our destination
                else if (destinationReached())
                {
                    setStateIdle();
                }
                //If we've been wandering for long enough, just stop.
                else if (state == EnemyState.WANDER && timer.Elapsed() > maxWanderTime)
                {
                    setStateIdle();
                }
                break;
            case EnemyState.PERSUE:
                //If we've lost the player, idle.
                if (!canSeePlayer())
                {
                    setStateIdle();
                }
                else
                {
                    //Set destination to player's current position
                    m_nav.destination = player.transform.position;
                }
                break;
            default: break;
        }
    }

    private bool destinationReached()
    {
        return Vector2.Distance(transform.position, m_nav.destination) < m_nav.stoppingDistance;
    }

    private void setStateIdle()
    {
        state = EnemyState.IDLE;
        maxIdleDelay = Random.Range(0.5f, 5f);
        timer.Reset();
        m_nav.destination = gameObject.transform.position;
    }

    private void setStateWander()
    {
        //Set destination, navmesh agent will lhandle the rest (see PlayerAI setState and Sneaking)
        state = EnemyState.WANDER;
        timer.Reset();
        GameObject randGoal;
        do
        {
            int randIndex = Random.Range(0, goals.Count);
            randGoal = goals[randIndex];

        } while(Vector2.Distance(transform.position, randGoal.transform.position) < m_nav.stoppingDistance);

        m_nav.destination = randGoal.transform.position;

    }

    private void setStatePersue()
    {
        //set destination to player's location
        state = EnemyState.PERSUE;
        m_nav.destination = player.transform.position;
    }

    private bool canSeePlayer()
    {
        Vector2 playerVector = (player.transform.position - gameObject.transform.position).normalized;
        //Debug.Log("Player Direction: " + playerVector.ToString());

        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        RaycastHit2D vectorHit = Physics2D.Raycast(gameObject.transform.position, playerVector, maxHearingDistance);
        gameObject.GetComponent<BoxCollider2D>().enabled = true;

        if (vectorHit && vectorHit.collider.gameObject.tag == "Player")
        {
            return true;
        }
        else return false;
    }
}

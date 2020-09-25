using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]

public class EnemyAI : MonoBehaviour
{
    NavMeshAgent2D m_nav;
    [SerializeField]
    List<GameObject> goals;
    private int currentGoalIndex = 0;
    private bool isReversePatroling = false;

    private enum EnemyState { IDLE, WANDER, PERSUE, INVESTIGATE };
    private EnemyState state;
    private QuickTimer timer;
    private float idleDelay;
    //private float maxWanderTime = 8; //8s


    public float maxIdleDelay = 4f;
    public float minIdleDelay = .5f;
    public bool followWaypointsInOrder = false;
    public float maxHearingDistance = 1f;
    public float maxVisionDistance = 2.5f;
    public float visionConeAngle = 45; 

    GameObject player;
    List<Vector3> pointers;
    Vector3 lastSeenPointer;

    // Start is called before the first frame update
    void Start()
    {
        timer = new QuickTimer();
        pointers = new List<Vector3>();
        maxIdleDelay = Random.Range(minIdleDelay, maxIdleDelay);
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
                if (canSeePlayer()) { setStatePersue(); }
                else if (canSeePointer()) { setStateInvestigate(); }
                else if (timer.Elapsed() > maxIdleDelay) //todo: make the max random
                {
                    setStateWander();
                }
                //else Leave destination set to self and continue
                break;
            case EnemyState.WANDER:
                if(canSeePlayer()) { setStatePersue(); }
                else if (canSeePointer()) { setStateInvestigate(); }
                //If we've reached our destination
                else if (destinationReached())
                {
                    setStateIdle();
                }
                //If we've been wandering for long enough, just stop.
                /*else if (state == EnemyState.WANDER && timer.Elapsed() > maxWanderTime)
                {
                    setStateIdle();
                }*/
                break;
            case EnemyState.PERSUE:
                bool seePlayer = canSeePlayer();
                //If we've lost the player, idle.
                if (!seePlayer && destinationReached())
                {
                    setStateIdle();
                }
                else if(seePlayer)
                {
                    //Set destination to player's current position
                    m_nav.destination = player.transform.position;
                }
                break;
            case EnemyState.INVESTIGATE:
                //If we see a pointer, we'll go into this state
                if (canSeePlayer()) { setStatePersue(); }
                else if (destinationReached())
                {
                    setStateIdle();
                }
                //else keep going
                break;
            default: break;
        }
    }

    //Caught the player!
    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("Enemy caught the player!");
        if (collision.gameObject.tag == "Player")
        {
            GameObject.Destroy(collision.gameObject);
        }
    }

    private bool destinationReached()
    {
        return Vector2.Distance(transform.position, m_nav.destination) <= m_nav.stoppingDistance;
    }

    private void setStateIdle()
    {
        state = EnemyState.IDLE;
        maxIdleDelay = Random.Range(minIdleDelay, maxIdleDelay);
        timer.Reset();
        m_nav.destination = gameObject.transform.position;
    }

    private void setStateWander()
    {
        //Set destination, navmesh agent will lhandle the rest (see PlayerAI setState and Sneaking)
        state = EnemyState.WANDER;
        timer.Reset();

        m_nav.destination = getNextWaypoint();
    }

    private void setStatePersue()
    {
        //set destination to player's location
        state = EnemyState.PERSUE;
        m_nav.destination = player.transform.position;
    }

    private void setStateInvestigate()
    {
        //Set destination to the pointer's location that it saw
        state = EnemyState.INVESTIGATE;
        m_nav.destination = lastSeenPointer;
    }

    //Pick the next waypoint to wander to
    private Vector3 getNextWaypoint()
    {
        if (followWaypointsInOrder)
        {
            if (currentGoalIndex >= goals.Count) currentGoalIndex = 0;
            else currentGoalIndex++;
            /*if (isReversePatroling)
            {
                currentGoalIndex--;
                if (currentGoalIndex == 0) isReversePatroling = false;
            }
            else
            {
                currentGoalIndex++;
                if (currentGoalIndex == (goals.Count - 1)) isReversePatroling = true;
            }*/
            return goals[currentGoalIndex].transform.position;
        }
        else
        {
            GameObject randGoal;
            do
            {
                int randIndex = Random.Range(0, goals.Count);
                randGoal = goals[randIndex];

            } while (Vector2.Distance(transform.position, randGoal.transform.position) < m_nav.stoppingDistance);

            return randGoal.transform.position;
        }
    }

    private bool canSeePlayer()
    {
        if (Vector2.Distance(player.transform.position, gameObject.transform.position) < maxVisionDistance)
        {
            Vector2 playerVector = (player.transform.position - gameObject.transform.position).normalized;
            //Debug.Log("Player Direction: " + playerVector.ToString());

            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            RaycastHit2D vectorHit = Physics2D.Raycast(gameObject.transform.position, playerVector, maxVisionDistance);
            gameObject.GetComponent<BoxCollider2D>().enabled = true;

            if (vectorHit && vectorHit.collider.gameObject.tag == "Player")
            {
                float angle = Vector2.Angle(gameObject.transform.right, playerVector);
                if (angle < visionConeAngle) return true;
                else if (vectorHit.distance < maxHearingDistance) return true;
            }
        }
        return false;
    }

    private bool canSeePointer()
    {
        foreach(var point in pointers)
        {
            if(canSeePointer(point)) return true;
        }
        return false;
    }

    private bool canSeePointer(Vector3 point)
    {
        if (Vector2.Distance(point, gameObject.transform.position) < maxVisionDistance)
        {
            //Get direction to the point
            Vector2 pointVector = (point - gameObject.transform.position).normalized;
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            RaycastHit2D vectorHit = Physics2D.Raycast(gameObject.transform.position, pointVector, maxVisionDistance);
            gameObject.GetComponent<BoxCollider2D>().enabled = true;

            //If the vector hits, it's a pointer, and it's the same pointer we're looking for
            if (vectorHit && vectorHit.collider.gameObject.tag == "Pointer" && vectorHit.transform.position == point)
            {
                float angle = Vector2.Angle(gameObject.transform.right, pointVector);
                if (angle < visionConeAngle)
                {
                    lastSeenPointer = point;
                    return true;
                }
            }
        }
        return false;
    }

    public void CoinDropped(Transform transform)
    {
        Debug.Log("CoinDropped");
        pointers.Add(transform.position);
    }

    public void CoinDestoryed(Transform transform)
    {
        Debug.Log("CoinDestroyed");
        pointers.Remove(transform.position);
    }
}

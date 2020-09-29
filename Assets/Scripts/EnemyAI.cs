using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(BoxCollider2D))]

//[RequireComponent(typeof(VisionCone))]

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
    Animator m_animator;

    GameObject alertSprite;
    Animator alert_animator;

    List<Vector3> pointers;
    Vector3 lastSeenPointer;

    Vector2 lastMoveDir = -Vector2.up;
    VisionCone vision;

    [SerializeField] private bool useOnlyMinIdle = true;

    // Start is called before the first frame update
    void Start()
    {
        timer = new QuickTimer();
        pointers = new List<Vector3>();
        maxIdleDelay = useOnlyMinIdle ? minIdleDelay : Random.Range(minIdleDelay, maxIdleDelay);
        m_nav = gameObject.GetComponent<NavMeshAgent2D>();

        m_animator = gameObject.GetComponent<Animator>();

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

        alertSprite = transform.Find("AlertSprite").gameObject;
        alert_animator = alertSprite.GetComponent<Animator>();

        vision = GetComponentInChildren<VisionCone>();
        vision.viewDistance = maxVisionDistance;
        vision.fov = visionConeAngle * 2;

        Vector3 hearingCircleScale = transform.Find("VisionCircle").localScale;
        hearingCircleScale.x = 2f * maxHearingDistance;
        hearingCircleScale.y = 2f * maxHearingDistance;
        transform.Find("VisionCircle").localScale = hearingCircleScale;

    }

    // Update is called once per frame
    void Update()
    {
        if (m_nav.velocity != Vector2.zero) lastMoveDir = m_nav.velocity.normalized;
        Debug.Log("Enemy in state " + state.ToString() + "and Player position is "+player.transform.position.ToString());
        Debug.DrawRay(transform.position, lastMoveDir * maxVisionDistance,Color.white);
        switch (state)
        {
            case EnemyState.IDLE:
                if (canSeePlayer()) { setStatePersue(); }
                else if (canSeePointer()) { setStateInvestigate(); }
                else if (timer.Elapsed() > maxIdleDelay) //todo: make the max random
                {
                    setStateWander();
                }
                m_animator.Play("Entry");
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

    private void FixedUpdate()
    {

        m_animator.SetFloat("VerticalSpeed", m_nav.velocity.normalized.y);
        m_animator.SetFloat("HorizontalSpeed", m_nav.velocity.normalized.x);

        if (m_nav.velocity != Vector2.zero)
        {
            m_animator.SetFloat("LastVerticalSpeed", m_nav.velocity.normalized.y);
            m_animator.SetFloat("LastHorizontalSpeed", m_nav.velocity.normalized.x);
        }

        
        vision.setAimDirection(lastMoveDir);
        vision.setOrigin(m_nav.transform.position);

    }

    //Caught the player!
    private bool playerCaught = false;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("Enemy caught the player!");
        if (collision.gameObject.tag == "Player")
        {
            playerCaught = true;
            collision.gameObject.SendMessage("caughtPlayer");
            setStateIdle();
            
        }
    }

    private bool destinationReached()
    {
        return Vector2.Distance(transform.position, m_nav.destination) <= m_nav.stoppingDistance;
    }

    private void setStateIdle()
    {
        state = EnemyState.IDLE;
        maxIdleDelay = useOnlyMinIdle ? minIdleDelay : Random.Range(minIdleDelay, maxIdleDelay);
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
        if(!playerCaught)
            alert_animator.Play("Alert");
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

            Vector3 nextgoal = goals[currentGoalIndex].transform.position;
            currentGoalIndex++;
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
            return nextgoal;
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
            Debug.DrawRay(transform.position, playerVector, Color.green);
            gameObject.GetComponent<BoxCollider2D>().enabled = true;

            if (vectorHit && vectorHit.collider.gameObject.tag == "Player")
            {
                float angle = Vector2.Angle(lastMoveDir, playerVector);
                print(angle);
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
                float angle = Vector2.Angle(lastMoveDir, pointVector);
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

    public void CoinDestroyed(Transform transform)
    {
        Debug.Log("CoinDestroyed");
        pointers.Remove(transform.position);
    }
}

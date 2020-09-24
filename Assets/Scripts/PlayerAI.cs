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

    Animator m_animator;

    NavMeshAgent2D m_nav;
    List<GameObject> goals;

    enum PlayerStates { IDLE, SNEAKING, DISTRACTED, CAUGHT};
    PlayerStates state;

    QuickTimer idleTimer;
    private float maxIdleDelay = 3f;


    List<Transform> coinLocations;

    private void Start()
    {
        idleTimer = new QuickTimer();

        m_nav = gameObject.GetComponent<NavMeshAgent2D>();
        goals = new List<GameObject>();
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Treasure"))
        {
            goals.Add(g);
            print(g.name);
        }
        ChangePlayerState(PlayerStates.IDLE);
        m_animator = GetComponent<Animator>();

        coinLocations = new List<Transform>();
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
                //print("max vs elapsed" + maxIdleDelay + "," + idleTimer.Elapsed());
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
                if(minDest != null)
                {
                    minDistance = Vector2.Distance(transform.position, minDest.transform.position);
                }
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
                w.z = -1;
                GameObject.Instantiate(pointer, w, pointer.transform.rotation);
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

        m_animator.SetFloat("VerticalSpeed", m_nav.velocity.normalized.y);
        m_animator.SetFloat("HorizontalSpeed", m_nav.velocity.normalized.x);

        if(m_nav.velocity != Vector2.zero)
        {
            m_animator.SetFloat("LastVerticalSpeed", m_nav.velocity.normalized.y);
            m_animator.SetFloat("LastHorizontalSpeed", m_nav.velocity.normalized.x);
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

            case PlayerStates.DISTRACTED:
            case PlayerStates.SNEAKING:
            case PlayerStates.CAUGHT:
                state = newState;
                break;
        }
    }


    public void RemoveGoal(GameObject g)
    {
        goals.Remove(g);
        minDest = null;
        minDistance = float.PositiveInfinity;
        GameObject.Destroy(g);
        ChangePlayerState(PlayerStates.IDLE);
    }
    

    public void CoinDropped(Transform t)
    {
        coinLocations.Add(t);
    }

    public void CoinDestroyed(Transform t)
    {
        coinLocations.Remove(t);
    }
}

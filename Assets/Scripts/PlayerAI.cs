using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

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

    GameProgressTracker gameManager;

    AudioSource a_source;


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

        GameObject gm = GameObject.FindGameObjectWithTag("Manager");
        gameManager = gm != null ? gm.GetComponent<GameProgressTracker>() : null;

        if (gameManager != null) if (gameManager.canReturn)
            {
                GameObject pos = GameObject.Find("ReturnPosition");
                m_nav.Warp(pos.transform.position);
                GameObject g = GameObject.Find("Exit");
                goals.Add(g);
                g.GetComponent<doorController>().enabled = true;
            }

        a_source = gameObject.GetComponent<AudioSource>();

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
                if (coinLocations.Count == 0)
                {
                    if (Vector2.Distance(transform.position, m_nav.destination) < m_nav.stoppingDistance)
                    {
                        ChangePlayerState(PlayerStates.IDLE);
                    }
                }
                else
                {
                    m_nav.destination = getMinCoin();
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

    private Vector2 getMinCoin()
    {
        float min = float.PositiveInfinity;
        Vector2 minPos = Vector2.zero;

        foreach(Transform g in coinLocations)
        {
            float d = Vector2.Distance(transform.position, g.position);
            if ( d < min)
            {
                min = d;
                minPos = g.position;
            }
        }
        return minPos;
    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 w = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            
            if (GameObject.Find("Walls").GetComponent<Tilemap>().GetTile(GameObject.Find("Grid").GetComponent<Grid>().WorldToCell(w)) != null) return;

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
                maxIdleDelay = UnityEngine.Random.Range(2f, 5f);
                break;

            case PlayerStates.DISTRACTED:
            case PlayerStates.SNEAKING:
            case PlayerStates.CAUGHT:
                state = newState;
                break;
        }
    }


    public AudioClip collectSound;

    public void RemoveGoal(GameObject g)
    {
        goals.Remove(g);
        minDest = null;
        minDistance = float.PositiveInfinity;

        if (gameManager != null)
            gameManager.PlayerGotATreasure(g);

        a_source.PlayOneShot(collectSound);

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


    public GameObject trapFront, trapBack;
    public void caughtPlayer()
    {
        if (state != PlayerStates.CAUGHT)
        {
            GameObject.Instantiate(trapFront, gameObject.transform);
            GameObject.Instantiate(trapBack, gameObject.transform);
        }

        ChangePlayerState(PlayerStates.CAUGHT);
        m_animator.Play("Caught");

        Invoke("reloadScene", 2f);
    }

    void reloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

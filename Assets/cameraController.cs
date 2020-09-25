using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    [SerializeField]
    private int StartFollowPosition, EndFollowPosition;
    private Vector3 startPosition;

    private GameObject player;


    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    public bool reverse = true;
    void Update()
    {
        if (!reverse)
        {
            if (player.transform.position.y > StartFollowPosition && player.transform.position.y < EndFollowPosition)
            {
                transform.position = startPosition + new Vector3(0, player.transform.position.y - StartFollowPosition, 0);
            }
        }

        else
        {
            if (player.transform.position.y > StartFollowPosition && player.transform.position.y < EndFollowPosition)
            {
                Vector3 position = new Vector3(transform.position.x, player.transform.position.y, -100);
                transform.position = position;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameProgressTracker : MonoBehaviour
{
    [SerializeField]
    private AudioClip backgroundMusic;

    [SerializeField]
    private int NumberOfTreasures;

    private int CollectedTreasures = 0;
    public bool canReturn = false;
    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Manager");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void PlayerGotATreasure()
    {
        CollectedTreasures++;
        if(CollectedTreasures == NumberOfTreasures)
        {
            canReturn = true;
        }
    }

    void resetGame()
    {
        CollectedTreasures = 0;
        canReturn = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameProgressTracker : MonoBehaviour
{
    [SerializeField]
    private AudioClip backgroundMusic;

    [SerializeField]
    private int NumberOfTreasures;


    private Image[] trophies;

    private int CollectedTreasures = 0;
    public bool canReturn = false;
    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Manager");
        GameObject canvas = transform.Find("Canvas").gameObject;
        trophies = canvas.transform.GetComponentsInChildren<Image>();

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);

        if(SceneManager.GetActiveScene().name == "Main Menu")
        {
            this.resetGame();
        }
    }

    // Update is called once per frame
    public void PlayerGotATreasure(GameObject g)
    {
        trophies[CollectedTreasures].sprite = g.GetComponent<SpriteRenderer>().sprite;
        Color newColor = trophies[CollectedTreasures].color;
        newColor.a = 0;
        trophies[CollectedTreasures].color = newColor;

        CollectedTreasures++;
        if (CollectedTreasures == NumberOfTreasures)
        {
            canReturn = true;
        }

    }

    public void resetGame()
    {
        CollectedTreasures = 0;
        canReturn = false;

        foreach (Image r in trophies)
        {
            Color newColor = Color.black;
            newColor.a = .5f;
            r.color = newColor;
        }
    }
}

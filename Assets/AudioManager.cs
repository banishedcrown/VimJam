using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public AudioClip clip; 

    public void playBurst()
    {
        gameObject.GetComponent<AudioSource>().PlayOneShot(clip);
    }

}

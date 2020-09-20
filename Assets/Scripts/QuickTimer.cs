using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickTimer
{
    private float startedTime;
    // Start is called before the first frame update

    public QuickTimer()
    {
        Start();
    }
    public void Start()
    {
        startedTime = Time.time;
    }

    public float Elapsed()
    {
        return Time.time - startedTime;
    }

    public void Reset()
    {
        Start();
    }
}

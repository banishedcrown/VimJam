using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickTimer
{
    private float startedTime;
    // Start is called before the first frame update

    public QuickTimer()
    {
        startedTime = Time.time;
    }
    public float Elapsed()
    {
        return Time.time - startedTime;
    }

    public void Reset()
    {
        startedTime = Time.time;
    }
}

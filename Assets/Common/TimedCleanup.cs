using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedCleanup : MonoBehaviour
{
    [SerializeField] private float delay = 5.0f;
    [SerializeField] private bool releaseChildren;

    private float start;

    private void Start()
    {
        start = Time.time;
    }

    private void Update()
    {
        if(Time.time > start + delay)
        {
            if (releaseChildren)
            {
                transform.DetachChildren();
            }
            Destroy(gameObject);
        }
    }
}

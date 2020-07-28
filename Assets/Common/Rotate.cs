using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private bool runOnFixedUpdate = false;
    [SerializeField] private Vector3 speed;

    private void Update()
    {
        if (!runOnFixedUpdate)
        {
            transform.Rotate(speed * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (runOnFixedUpdate)
        {
            transform.Rotate(speed);
        }
    }
}

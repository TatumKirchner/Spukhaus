using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    public float speed = 10;
    [SerializeField] private bool x = false;
    [SerializeField] private bool y = false;
    [SerializeField] private bool z = false;

    // Put on an object that you want to spin
    void Update()
    {
        if (y)
            transform.Rotate(Vector3.up * speed * Time.deltaTime);
        if (x)
            transform.Rotate(Vector3.right * speed * Time.deltaTime);
        if (z)
            transform.Rotate(Vector3.forward * speed * Time.deltaTime);
    }
}

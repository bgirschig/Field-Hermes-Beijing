// Move the gameObject at a fixed speed;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Auto_move : MonoBehaviour
{
    public Vector3 speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(speed * Time.deltaTime);
    }
}

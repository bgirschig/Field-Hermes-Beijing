using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwing : MonoBehaviour
{
    public Detector detector;

    void Start()
    {
        
    }

    void Update()
    {
        transform.Translate(new Vector3(0,0,detector.speed));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwing : MonoBehaviour
{
    public Detector detector;
    public float speed = 1;
    public float forwardBias = 1.5f;
    public float smoothTime = 0.1f;

    Vector3 direction = new Vector3(0,0,1);
    
    float targetPosition = 0;
    float smoothedPosition = 0;
    float prevSmoothedPosition = 0;
    float currentVelocity = 0;
    float maxSpeed = 30;

    void Start()
    {
    }

    void Update()
    {
        if (detector.speed > 0) {
            targetPosition += -detector.speed * speed * forwardBias;
        } else {
            targetPosition += -detector.speed * speed;
        }

        prevSmoothedPosition = smoothedPosition;
        smoothedPosition = Mathf.SmoothDamp(smoothedPosition, targetPosition, ref currentVelocity, smoothTime);
        transform.Translate(0, 0, prevSmoothedPosition - smoothedPosition);
    }

    // void OnGUI()
    // {
    //     var style = new GUIStyle();
    //     style.fontSize = Screen.width/20;
    //     style.normal.textColor = new Color(255,255,255);
    //     GUI.TextArea(
    //         new UnityEngine.Rect(10, 10, 200, 50),
    //         $"{detector.speed}\n{detector.smoothedPosition}",
    //         style
    //     );
    // }
}

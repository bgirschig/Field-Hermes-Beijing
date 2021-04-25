using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class CameraSwing : MonoBehaviour
{
    public Detector detector;
    public Mode mode;

    [Header("Main settings")]
    [Tooltip("General speed for the camera. Applies when going forward and backward")]
    public float overallSpeed = 1;
    [Tooltip("How much faster we should move when going forward")]
    public float forwardBias = 1.5f;
    [Tooltip("Higher smooth values will make the movement smoother (duh) but also less responsive")]
    public float smoothTime = 0.1f;
    [Tooltip("When holding the shift key down, all speed settings will be multiplied by this much")]
    public float shiftMultiplier = 6;
    [Tooltip("Max speed in any direction. Applies to all speeds")]
    public float maxSpeed = 6;

    [Header("Emulator settings")]
    [Tooltip("Simulated length of the pendulum for the sine emulator")]
    public float pendulumLength = 2;
    [Tooltip("Adjust the speed of the linear forward emulator. This is combined with the overallSpeed setting")]
    public float linearForwartSpeed = 0.1f;
    
    float targetPosition = 0;
    float smoothedPosition = 0;
    float prevSmoothedPosition = 0;
    float currentVelocity = 0;
    float gravity = 9.81f;

    void Update()
    {
        float speed = 0;
        switch (mode)
        {
            case Mode.DETECTOR:
                speed = detector.speed;
                break;
            case Mode.LINEAR_FORWARD:
                speed = linearForwartSpeed;
                break;
            case Mode.SINE_EMULATOR:
                float period = 2 * Mathf.PI * Mathf.Sqrt(pendulumLength / gravity);
                float amplitude = 1;
                speed = Mathf.Sin(2 * Mathf.PI * Time.time / period) * (amplitude / period);
                break;
            case Mode.KEYBOARD:
                if (EventSystem.current.currentSelectedGameObject != null) break;

                if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.RightArrow)) {
                    speed = linearForwartSpeed;
                } else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow)) {
                    speed = -linearForwartSpeed;
                    // compensate forward bias in keyboard mode, so that foward and backward speeds are the same
                    speed *= forwardBias;
                }
                break;
            default:
                Debug.LogError("Unexpected camera swing mode");
                break;
        }

        bool shiftPressed = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
            EventSystem.current.currentSelectedGameObject == null;
        if (shiftPressed) speed *= shiftMultiplier;

        if (speed > maxSpeed) speed = maxSpeed;
        else if (speed < -maxSpeed) speed = -maxSpeed;

        if (speed > 0) {
            targetPosition += speed * overallSpeed * forwardBias * Time.deltaTime;
        } else {
            targetPosition += speed * overallSpeed * Time.deltaTime;
        }

        prevSmoothedPosition = smoothedPosition;
        smoothedPosition = Mathf.SmoothDamp(smoothedPosition, targetPosition, ref currentVelocity, smoothTime);
        transform.Translate(0, 0, smoothedPosition - prevSmoothedPosition);
    }

    public void setMode(string modeName) {
        mode = (Mode)Enum.Parse( typeof(Mode), modeName );
    }

    public static List<string> modes {
        get {
            var values = (Mode[])Enum.GetValues(typeof(Mode));
            List<string> output = new List<string>();
            for (int i = 0; i < values.Length; i++) output.Add(values[i].ToString());
            return output;
        }
    }

    public enum Mode {
        DETECTOR, LINEAR_FORWARD, SINE_EMULATOR, KEYBOARD
    }
}

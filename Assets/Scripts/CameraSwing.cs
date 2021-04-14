using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Emulator settings")]
    [Tooltip("Simulated length of the pendulum for the sine emulator")]
    public float pendulumLength = 2;
    [Tooltip("Adjust the speed of the emulators. This is combined with the overallSpeed setting")]
    public float emulatorSpeed = 0.1f;
    
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
                speed = emulatorSpeed;
                break;
            case Mode.SINE_EMULATOR:
                float period = 2 * Mathf.PI * Mathf.Sqrt(pendulumLength / gravity);
                float amplitude = 1;
                speed = Mathf.Sin(2 * Mathf.PI * Time.time / period) * (amplitude / period);
                break;
            default:
                Debug.LogError("Unexpected camera swing mode");
                break;
        }

        bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (shiftPressed) speed *= shiftMultiplier;

        if (speed > 0) {
            targetPosition += speed * overallSpeed * forwardBias;
        } else {
            targetPosition += speed * overallSpeed;
        }

        prevSmoothedPosition = smoothedPosition;
        smoothedPosition = Mathf.SmoothDamp(smoothedPosition, targetPosition, ref currentVelocity, smoothTime);
        transform.Translate(0, 0, smoothedPosition - prevSmoothedPosition);
    }

    public enum Mode {
        DETECTOR, LINEAR_FORWARD, SINE_EMULATOR
    }
}

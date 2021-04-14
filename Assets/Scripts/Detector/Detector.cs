// Detects the horizontal position of a bright(er) object in a webcam input

using UnityEngine;

using System;
using System.IO;
using UnityEngine.Events;

public class Detector : MonoBehaviour
{
    [NonSerialized]
    public DetectorCore detectorCore;

    // Inspector settings
    public SharedWebcam webcam;

    // output
    [NonSerialized]
    public float speed = 0;
    [NonSerialized]
    public float smoothedSpeed = 0;
    [NonSerialized]
    public UnityEvent onMaskChange = new UnityEvent();
    public float smoothedPosition;

    // Textures
    [NonSerialized]
    public Texture debugTexture; // In order to access the debug image from other components, it
                                 // needs to be in the 'Texture' format we'll do the conversion
                                 // after the detector is done with a frame and before starting the
                                 // next one

    // Smoothing
    float lastDetectionTime = 0;
    //   position
    float lastRawPosition = 0;
    float positionSmoothSpeed = 0;
    float positionSmoothDuration = 0.01f;

    // Start is called before the first frame update
    void Start()
    {
        detectorCore = new DetectorCore();

        webcam.onCameraChange.AddListener(OnCameraChange);
        if (webcam.ready) OnCameraChange();

        LoadMaskFromDisk();
    }

    // Update is called once per frame
    void Update()
    {
        if (!enabled) return;
        
        if (detectorCore.debugImg != null) {
            debugTexture = OpenCvSharp.Unity.MatToTexture(detectorCore.debugImg);
        }
        if (webcam.didUpdateThisFrame) {
            float deltaTime = Time.time - lastDetectionTime;
            lastDetectionTime = Time.time;
            float position = detectorCore.detect(OpenCvSharp.Unity.TextureToMat(webcam.capture));

            float rawSpeed = (position - lastRawPosition) / deltaTime;
            speed = rawSpeed;
            lastRawPosition = position;
        }

        smoothedPosition = Mathf.SmoothDamp(smoothedPosition, lastRawPosition, ref positionSmoothSpeed, positionSmoothDuration);
    }

    void OnCameraChange() {
        detectorCore.SetSize(webcam.capture.width, webcam.capture.height);
        onMaskChange.Invoke();
    }

    public void UpdateMask() {
        detectorCore.UpdateMask();
        onMaskChange.Invoke();
    }

    public void SaveMask() {
        Texture2D texture = OpenCvSharp.Unity.MatToTexture(detectorCore.srcMask);
        var pngBytes = texture.EncodeToPNG();
        string destination = Path.Combine(Application.persistentDataPath, "mask.png");
        File.WriteAllBytes(destination, pngBytes);
    }

    void LoadMaskFromDisk() {
        string maskPath = Path.Combine(Application.persistentDataPath, "mask.png");
        if (File.Exists(maskPath)) {            
            var pngBytes = File.ReadAllBytes(maskPath);
            var loaderTexture = new Texture2D(2,2);
            loaderTexture.LoadImage(pngBytes);
            
            detectorCore.setMask(OpenCvSharp.Unity.TextureToMat(loaderTexture));
            onMaskChange.Invoke();
        }
    }
}
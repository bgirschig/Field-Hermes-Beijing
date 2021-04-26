/// Manage references to camera texture and unity camera management
// This wraps around unity's webcam texture and fixes a few issues with it:
// - Multiple components can't create a WebCamTexture from the same camera. This creates one that
//   other components can reference
// - The texture is not always ready immediately after the WebCamTexture is created. SharedWebcam
//   has a 'onCameraChange' event is invoked at the appropriate time

using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class SharedWebcam : MonoBehaviour
{
    public int defaultCameraIndex;
    public bool autoStart;

    [NonSerialized]
    public UnityEvent onCameraChange = new UnityEvent();

    [NonSerialized]
    public string currentCamera = null;
    [NonSerialized]
    public WebCamTexture capture;
    [NonSerialized]
    public float ratio;
    [NonSerialized]
    public int width;
    [NonSerialized]
    public int height;
    [NonSerialized]
    public bool ready = false;

    public bool didUpdateThisFrame {
        get {
            if (!ready) return false;
            if (capture == null) return false;
            return capture.didUpdateThisFrame;
        }
    }
    
    public List<string> devices {
        get {
            List<string> output = new List<string>();
            foreach (var device in WebCamTexture.devices) output.Add(device.name);     
            return output;
        }
    }

    private bool waitingCameraInit = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!autoStart) return;
        if (currentCamera != null) setCamera(currentCamera);
        else setCamera(defaultCameraIndex);
    }

    void Update() {
        if (waitingCameraInit && capture.width > 100) {
            width = capture.width;
            height = capture.height;
            ratio = (float)width / height;
            onCameraChange.Invoke();
            
            waitingCameraInit = false;
            ready = true;
            
            // At this point, the capture should already be playing, but in some cases
            // (some cameras, on startup) we need to run Play() again.
            if(!capture.isPlaying) capture.Play();
        }
    }

    public void setCamera(string deviceName) {
        Debug.Log($"Set camera: {deviceName}");
        if (capture) capture.Stop();
        if (deviceName == currentCamera) return;
        if (deviceName == null) return;
        ready = false;
        capture = new WebCamTexture(deviceName);
        capture.Play();
        currentCamera = deviceName;
        waitingCameraInit = true;
    }

    public void setCamera(int deviceIndex) {
        int deviceCount = WebCamTexture.devices.Length;
        deviceIndex %= deviceCount;

        string deviceName = WebCamTexture.devices[deviceIndex].name;
        setCamera(deviceName);
    }
}

/// Display the output from the first webcam on a rawImage

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(AspectRatioFitter))]
public class WebcamView : MonoBehaviour
{
    public SharedWebcam source;
    RawImage image;
    AspectRatioFitter aspectRatioFitter;

    void Start() {
        image = GetComponent<RawImage>();
        aspectRatioFitter = GetComponent<AspectRatioFitter>();
        source.onCameraChange.AddListener(updateTexture);
        if (source.ready) updateTexture();
    }

    void updateTexture() {
        print("updateTexture");
        image.texture = source.capture;
        aspectRatioFitter.aspectRatio = source.ratio;
    }
}
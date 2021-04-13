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

    IEnumerator Start() {
        image = GetComponent<RawImage>();
        aspectRatioFitter = GetComponent<AspectRatioFitter>();
        
        yield return 0; // wait for 'source' to create its event listeners
        source.onCameraChange.AddListener(updateTexture);
    }

    void updateTexture() {
        image.texture = source.capture;
        aspectRatioFitter.aspectRatio = source.ratio;
    }
}
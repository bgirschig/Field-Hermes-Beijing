using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
[ExecuteInEditMode]
public class VideoScaler : MonoBehaviour
{
    public Axis scaleAxis;
    VideoPlayer player;
    
    void Start() {
        updateScale();
    }

    #if UNITY_EDITOR
    void Update() {
        updateScale();
    }
    #endif

    void updateScale() {
        if (player == null) player = GetComponent<VideoPlayer>();

        float aspectRatio = (float)player.width / player.height;
        float scale;
        switch (scaleAxis)
        {
            case Axis.Horizontal:
                scale = transform.localScale.y;
                transform.localScale = new Vector3(aspectRatio*scale, scale, scale);
                break;
            case Axis.Vertical:
                scale = transform.localScale.x;
                transform.localScale = new Vector3(scale, scale/aspectRatio, scale);
                break;
            default:
                break;
        }
    }

    public enum Axis {
        Horizontal, Vertical,
    }
}

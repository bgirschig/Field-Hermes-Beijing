using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class VideoBillboard : MonoBehaviour
{
    public VideoClip clip;
    [Range(0,1)]
    public float timeOffset;
    public float size = 1;
    public GameObject label;
    public float ratioOffset = 0;

    private VideoPlayer player;

    // Start is called before the first frame update
    void Start()
    {
        init();
        player.Play();
    }

    void init() {
        player = GetComponentInChildren<VideoPlayer>();

        player.clip = clip;
        setScale();
        setTimeOffset();
    }

    void setTimeOffset() {
        player.time = timeOffset * player.clip.length;
    }

    void setScale() {
        float aspectRatio = ((float)player.width / player.height);
        aspectRatio = aspectRatio * Mathf.Pow(2, ratioOffset);

        float width;
        float height;
        if (aspectRatio > 1) {
            width = size;
            height = width/aspectRatio;
        } else {
            height = size;
            width = height * aspectRatio;
        }

        player.transform.localScale = new Vector3(
            width,
            height,
            1
        );

        if (label) {
            Vector3 labelPosition = -player.transform.localScale * 0.5f;
            labelPosition.z = 0;
            label.transform.position = transform.TransformPoint(labelPosition);
        }
    }

    void OnValidate() {
        if (!isActiveAndEnabled) return;
        init();
        // player.Play();
        // player.Pause();
    }
}

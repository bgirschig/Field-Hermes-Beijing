// Scales the gameobject to match the attached video's ratio (assuming the mesh is square)

using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
[ExecuteInEditMode]
public class VideoScaler : MonoBehaviour
{
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
        float scale = transform.localScale.x;
        transform.localScale = new Vector3(scale, scale/aspectRatio, scale);
    }
}

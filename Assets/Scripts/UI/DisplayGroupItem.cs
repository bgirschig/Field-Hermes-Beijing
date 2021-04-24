// Sets the targetDisplay of the attached camera or canvas to a given value.
// used by 'DisplayManager' to work its magic

using UnityEngine;

public class DisplayGroupItem : MonoBehaviour
{
    public int displayGroup = 0;

    public void Start() {
        setTargetDisplay(displayGroup);
    }

    public void setTargetDisplay(int displayIdx) {
        Camera camera = GetComponent<Camera>();
        Canvas canvas = GetComponent<Canvas>();
        if (camera) {
            camera.targetDisplay = displayIdx;
        }
        if (canvas) {
            canvas.targetDisplay = displayIdx;
        }
        if (camera == null && canvas == null) {
            Debug.LogError($"Could not find a camera or canvas on {name}");
        }
    }
}

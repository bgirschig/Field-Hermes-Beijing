/// Display the debug output of a detector on a rawImage

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class DetectorView : MonoBehaviour
{
    [SerializeField]
    Detector detector;

    RawImage image;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        if (detector.debugTexture == null) return;
        image.texture = detector.debugTexture;
    }

    void OnGUI()
    {
        var style = new GUIStyle();
        style.fontSize = Screen.width/20;
        style.normal.textColor = new Color(255,255,255);
        GUI.TextArea(
            new UnityEngine.Rect(10, 10, 200, 50),
            string.Format("Detector framerate: {0:0.0}, {1:0.00}", detector.framerate, detector.position),
            style
        );
    }
}

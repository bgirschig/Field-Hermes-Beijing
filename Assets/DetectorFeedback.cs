/// UI element for displaying processed detector state (smoothed position, speed, etc...)

using System.Collections;
using UnityEngine;

public class DetectorFeedback : MonoBehaviour
{
    public Detector detector;
    public RectTransform positionCursor;
    public RectTransform speedCursor;
    
    float width;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return 0;
        width = ((RectTransform)transform).rect.width;
    }

    // Update is called once per frame
    void Update()
    {
        placeCursor(positionCursor, detector.smoothedPosition, 0, 1);
        placeCursor(speedCursor, detector.speed, -4, 4);
    }

    void placeCursor(RectTransform cursor, float value, float min, float max) {
        if (width == 0) return;
        float x = MathUtils.Map(value, min, max, -width/2, width/2);
        // print($"{value}, {min}, {max}, 0, {width} -> {x}");
        cursor.localPosition = new Vector3(x, 0, 0);
    }
}

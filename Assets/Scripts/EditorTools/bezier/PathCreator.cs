using UnityEngine;

public class PathCreator : MonoBehaviour {
    [HideInInspector]
    public BezierPath path;
    Vector3[] precomputedPoints;
    
    [Range(0,1)]
    public float timeOffset = 0.837f;

    void Start() {
        computePoints();
    }

    public void CreatePath() {
        path = new BezierPath();
    }

    public void computePoints(float spacing=0.1f, float resolution=1f) {
        precomputedPoints = path.CalcualteEvenlySpacedPoints(spacing, resolution);
    }

    public Vector3 GetPointAtTime(float time) {
        time = (time+timeOffset)%1;
        if (time >= 1) return transform.TransformPoint(precomputedPoints[precomputedPoints.Length - 1]);
        if (time <= 0) return transform.TransformPoint(precomputedPoints[0]);
        int segmentStartIndex = Mathf.FloorToInt((precomputedPoints.Length - 1) * time);
        int segmentEndIndex = segmentStartIndex + 1;
        float interval = 1f / precomputedPoints.Length;

        float segmentStartTime = segmentStartIndex * interval;
        float localTime = (time - segmentStartTime) / interval;

        Vector3 localPoint = Vector3.Lerp(precomputedPoints[segmentStartIndex], precomputedPoints[segmentEndIndex], localTime);
        return transform.TransformPoint(localPoint);
    }
}

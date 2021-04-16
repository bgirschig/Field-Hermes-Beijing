/// Generic, serializable bezier object

using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Bezier {
    [SerializeField]
    public List<Vector3> points;
    [SerializeField][Range(0.01f, 5)]
    public float spacing;
    [SerializeField][Range(0.01f, 5)]
    public float resolution;

    [NonSerialized]
    public Vector3[] precomputedPoints;

    public Bezier(float spacing=0.1f, float resolution=1) {
        this.spacing = spacing;
        this.resolution = resolution;

        // placeholder bezier
        points = new List<Vector3>() {
            Vector3.left,
            (Vector3.left + Vector3.up) * 0.5f,
            (Vector3.right + Vector3.down) * 0.5f,
            Vector3.right,
        };
        Recompute();
    }

    public Vector3 this[int index] {
        get { return points[index]; }
    }

    public int numPoints {
        get { return points.Count; }
    }

    public int numSegment {
        get { return (points.Count - 4) / 3 + 1; }
    }

    public void addSegment(Vector3 anchorPos) {
        points.Add(points[points.Count-1]*2 - points[points.Count-2]);
        points.Add((points[points.Count-1] + anchorPos) * .5f);
        points.Add(anchorPos);
        Recompute();
    }

    public Vector3[] GetPointInSegment(int i) {
        return new Vector3[] {
            points[i*3], points[i*3+1], points[i*3+2], points[i*3+3]
        };
    }

    public Vector3 GetPointAtTime(float time) {
        if (precomputedPoints == null) Recompute();
        if (time == 1) return precomputedPoints[precomputedPoints.Length - 1];
        if (time <= 0) return precomputedPoints[0];
        time = time%1;
        int segmentStartIndex = Mathf.FloorToInt((precomputedPoints.Length - 1) * time);
        int segmentEndIndex = segmentStartIndex + 1;
        float interval = 1f / precomputedPoints.Length;

        float segmentStartTime = segmentStartIndex * interval;
        float localTime = (time - segmentStartTime) / interval;

        Vector3 localPoint = Vector3.Lerp(precomputedPoints[segmentStartIndex], precomputedPoints[segmentEndIndex], localTime);
        return localPoint;
    }

    public void Recompute() {
        List<Vector3> evenlySpacedPoints = new List<Vector3>();
        evenlySpacedPoints.Add(points[0]);
        Vector3 previousPoint = points[0];
        float dstSinceLastEvenPoint = 0;

        for (int segmentIndex = 0; segmentIndex < numSegment; segmentIndex++) {
            Vector3[] p = GetPointInSegment(segmentIndex);
            float controlNetLength = Vector3.Distance(p[0], p[1]) + Vector3.Distance(p[1], p[2]) + Vector3.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector3.Distance(p[0], p[1]) + controlNetLength / 2f;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            float t = 0;
            while (t <= 1) {
                t += 1f / divisions;
                Vector3 pointOnCurve = EvaluateCubic(p[0], p[1], p[2], p[3], t);  
                dstSinceLastEvenPoint += Vector3.Distance(previousPoint, pointOnCurve);

                while (dstSinceLastEvenPoint >= spacing) {
                    float overshootDistance = dstSinceLastEvenPoint - spacing;
                    Vector3 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDistance;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    dstSinceLastEvenPoint = overshootDistance;
                    previousPoint = newEvenlySpacedPoint;
                }
                previousPoint = pointOnCurve;
            }
        }
        precomputedPoints = evenlySpacedPoints.ToArray();
    }

    static Vector3 EvaluateQuadratic(Vector3 a, Vector3 b, Vector3 c, float t) {
        Vector3 p0 = Vector3.Lerp(a, b, t);
        Vector3 p1 = Vector3.Lerp(b, c, t);
        return Vector3.Lerp(p0, p1, t);
    }

    static Vector3 EvaluateCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t) {
        Vector3 p0 = EvaluateQuadratic(a, b, c, t);
        Vector3 p1 = EvaluateQuadratic(b, c, d, t);
        return Vector3.Lerp(p0, p1, t);
    }
}
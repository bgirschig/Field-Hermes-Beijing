using UnityEngine;
using UnityEditor;
[System.Serializable]
public class BezierPath : MonoBehaviour {
    [SerializeField]
    [HideInInspector]
    public Bezier bezier;

    public void movePoint(int i, Vector3 newPosition) {
        Vector3 deltaMove = newPosition - bezier.points[i];
        bezier.points[i] = newPosition;

        if (i%3 == 0) {
            if (i+1 < bezier.points.Count) bezier.points[i + 1] += deltaMove;
            if (i-1 >= 0) bezier.points[i - 1] += deltaMove;
        } else {
            bool nextPointIsAnchor = (i+1) % 3 == 0;
            int correspondingControlIndex = nextPointIsAnchor ? i + 2 : i - 2;
            int anchorIndex = nextPointIsAnchor ? i + 1 : i - 1;

            if (correspondingControlIndex >= 0 && correspondingControlIndex < bezier.points.Count) {
                float dist = (bezier.points[anchorIndex] - bezier.points[correspondingControlIndex]).magnitude;
                Vector3 direction = (bezier.points[anchorIndex] - newPosition).normalized;

                bezier.points[correspondingControlIndex] = bezier.points[anchorIndex] + direction * dist;
            }
        }

        bezier.Recompute();
    }

    #if UNITY_EDITOR
    void OnDrawGizmos() {
        if (UnityEditor.Selection.activeGameObject == this.gameObject) {
            Gizmos.color = Color.green;
        } else {
            Gizmos.color = new Color(0,0.7f,0);
        }

        if (bezier.precomputedPoints == null) return;
        Vector3? prevPoint = null;
        Vector3 point;
        foreach(var localPoint in bezier.precomputedPoints) {
            point = transform.TransformPoint(localPoint);
            if (prevPoint != null) Gizmos.DrawLine((Vector3)prevPoint, point);
            prevPoint = point;
        }
    }
    #endif

    public Vector3 GetPointAtTime(float time) {
        return transform.TransformPoint(bezier.GetPointAtTime(time));
    }

    public Vector3? GetIntersection(Plane plane) {
        plane = transform.worldToLocalMatrix.TransformPlane(plane);
        Vector3? output = bezier.GetIntersection(plane);
        if (output == null) return output;
        else return transform.TransformPoint((Vector3)output);
    }

    void Reset() {
        bezier = new Bezier();
    }
}

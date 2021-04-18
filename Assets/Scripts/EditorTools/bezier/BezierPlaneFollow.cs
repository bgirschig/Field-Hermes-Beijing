/// Place a gameObject at the intersection between a bezier and a plane
// The plane is defined by the UP and RIGHT vectors of the target
// If there are multiple intersections (plane crosses the bezier in multiple points) the
// first match (following the bezier from start to end) is used

using UnityEngine;

[ExecuteInEditMode]
public class BezierPlaneFollow : MonoBehaviour
{
    public BezierPath bezierPath;
    public PlaneComponent target;

    // Update is called once per frame
    void Update()
    {
        if (bezierPath == null) return;
        if (target == null) return;

        Plane plane = target.GetPlane();
        Vector3? position = bezierPath.GetIntersection(plane);
        if (position != null) {
            transform.position = (Vector3)position;
        }
    }
}

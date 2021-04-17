/// target component for BezierPlaneFollow

using UnityEngine;

public class PlaneComponent : MonoBehaviour
{
    void OnDrawGizmos() {
        Gizmos.color = new Color(1,1,1,0.1f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, new Vector3(50,50,0.01f));
    }

    /// returns the plane struct representing this plane component
    public Plane GetPlane() {
        return new Plane(
            transform.position,
            transform.position + transform.right,
            transform.position + transform.up);
    }
}

// Update the "next" bezier in a sequence so that it matches though a portal

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierPortalMatcher))]
[CanEditMultipleObjects]
public class BezierPortalMatcherEditor : Editor 
{
    BezierPortalMatcher bezierPortalMatcher;

    void OnEnable() {
        bezierPortalMatcher = (BezierPortalMatcher)target;
    }

    void OnSceneGUI() {
        var portal = bezierPortalMatcher.portal;
        var nextBezier = bezierPortalMatcher.nextBezier;
        var bezierPath = bezierPortalMatcher.GetComponent<BezierPath>();

        if (portal != null && nextBezier != null) {
            // position of the last point of this bezier
            Vector3 lastPoint = bezierPath.bezier.points[bezierPath.bezier.points.Count-1];
            // world position of ...
            lastPoint = bezierPath.transform.TransformPoint(lastPoint);
            // relative position to the portal ...
            lastPoint = portal.transform.InverseTransformPoint(lastPoint);
            // world position relative to the portal target
            Vector3 firstPoint = portal.target.TransformPoint(lastPoint);

            nextBezier.bezier.points[0] = nextBezier.transform.InverseTransformPoint(firstPoint);

            Vector3 handleOffset = bezierPath.bezier.points[bezierPath.bezier.points.Count-1] - bezierPath.bezier.points[bezierPath.bezier.points.Count-2];
            nextBezier.bezier.points[1] = nextBezier.bezier.points[0] + handleOffset;

            nextBezier.bezier.Recompute();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BezierPortalMatcher : MonoBehaviour
{
    // most interesting stuff happens in the editor for this component

    BezierPath bezierPath;
    public Portal portal;
    public BezierPath nextBezier;

    #if UNITY_EDITOR
    void OnDrawGizmosSelected() {
        Gizmos.color = new Color(0,0.7f,0);
        Gizmos.matrix = nextBezier.transform.localToWorldMatrix * portal.target.worldToLocalMatrix * portal.transform.localToWorldMatrix;
        if (nextBezier != null) {
            nextBezier.DrawBezierGizmo();
        }
    }
    #endif
}

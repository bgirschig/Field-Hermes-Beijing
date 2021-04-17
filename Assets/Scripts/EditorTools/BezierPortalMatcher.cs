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
}

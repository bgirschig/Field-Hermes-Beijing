/// Allows using a group of bezier path as one, "teleporing" from one to the other, with a global 'time' parameter
// TODO: This is a text/exploration. delete once not needed anymore

using UnityEngine;

public class BezierChain : MonoBehaviour
{
    [Range(0,1)]
    public float time;
    BezierPath[] bezierPaths;
    
    // Start is called before the first frame update
    void Start()
    {
        bezierPaths = GetComponentsInChildren<BezierPath>();
    }

    void OnDrawGizmos() {
        Vector3 pos = GetPointAtTime(time);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pos, 1.5f);
    }

    void Reset() {
        bezierPaths = GetComponentsInChildren<BezierPath>();
    }

    void OnValidate() {
        bezierPaths = GetComponentsInChildren<BezierPath>();
    }

    Vector3 GetPointAtTime(float time) {
        if (time == 1) return bezierPaths[bezierPaths.Length-1].GetPointAtTime(1);
        time = time%1;
        if (time == 0) return bezierPaths[0].GetPointAtTime(0);

        float totalLength = 0;
        for (int i = 0; i < bezierPaths.Length; i++)
            totalLength += bezierPaths[i].bezier.precomputedPointsCount;

        float segmentStartTime = 0;
        float segmentLength = 0;
        int selectedPath = -1;
        for (int i = 0; i < bezierPaths.Length; i++) {
            segmentLength = bezierPaths[i].bezier.precomputedPoints.Length / totalLength;
            if (segmentStartTime + segmentLength >= time) {
                selectedPath = i;
                break;
            }
            segmentStartTime += segmentLength;
        }

        float timeInSegment = (time - segmentStartTime) / segmentLength;
        Vector3 position = bezierPaths[selectedPath].GetPointAtTime(timeInSegment);
        return position;
    }
}

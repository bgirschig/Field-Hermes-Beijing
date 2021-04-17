
using UnityEngine;

[ExecuteInEditMode]
public class FollowPath : MonoBehaviour {

    public BezierPath pathCreator;
    
    [Range(0,1)]
    public float time;

    void Update() {
        if (pathCreator != null) transform.position = pathCreator.GetPointAtTime(time);
    }
}
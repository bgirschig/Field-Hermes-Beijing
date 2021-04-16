
using UnityEngine;

[ExecuteInEditMode]
public class PathPlacer : MonoBehaviour {

    public PathCreator pathCreator;
    
    [Range(0,1)]
    public float time;

    void Update() {
        pathCreator.computePoints();
        transform.position = pathCreator.GetPointAtTime(time);
    }
}
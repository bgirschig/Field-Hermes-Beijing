using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTarget : MonoBehaviour
{
    void OnDrawGizmos() {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0.64f, 0.46f, 1, 1);
        Gizmos.DrawCube(Vector3.zero, new Vector3(1,2,0.1f));
    }
}

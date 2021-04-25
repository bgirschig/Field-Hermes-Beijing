using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 1;
    public PortalItem teleportNotifier;

    Vector3 prevTargetPosition;
    Vector3 currentVelocity;

    public bool antiReturn;
    public Vector3 antiReturnDirection = Vector3.forward;

    // Start is called before the first frame update
    void Start()
    {
        teleportNotifier.onTeleport.AddListener(OnTeleport);
    }

    // Update is called once per frame
    void Update()
    {
        if (antiReturn) {
            var direction = target.position - transform.position;
            if (Vector3.Dot(antiReturnDirection, direction) < 0) return;
        }
        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref currentVelocity, smoothTime);
    }

    void OnTeleport(Vector3 positionOffset) {
        transform.position += positionOffset;
    }
}

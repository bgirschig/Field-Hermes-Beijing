using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCamera : MonoBehaviour
{
    public Transform portal;
    public Transform otherPortal;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 positionOffset = otherPortal.transform.InverseTransformPoint(Camera.main.transform.position);
        // positionOffset.z = -positionOffset.z;
        transform.position = portal.transform.TransformPoint(positionOffset);

        Vector3 forwardOffset = otherPortal.transform.InverseTransformDirection(Camera.main.transform.forward);
        Vector3 upOffset = otherPortal.transform.InverseTransformDirection(Camera.main.transform.up);
        transform.rotation = Quaternion.LookRotation(
            portal.transform.TransformDirection(forwardOffset),
            portal.transform.TransformDirection(upOffset)
        );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
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
        otherPortal.transform.Rotate(0,180,0);

        Vector3 positionOffset = otherPortal.transform.InverseTransformPoint(Camera.main.transform.position);
        transform.position = portal.transform.TransformPoint(positionOffset);

        Vector3 forwardOffset = otherPortal.transform.InverseTransformDirection(Camera.main.transform.forward);
        Vector3 upOffset = otherPortal.transform.InverseTransformDirection(Camera.main.transform.up);
        transform.rotation = Quaternion.LookRotation(
            portal.transform.TransformDirection(forwardOffset),
            portal.transform.TransformDirection(upOffset)
        );

        otherPortal.transform.Rotate(0,180,0);
    }
}

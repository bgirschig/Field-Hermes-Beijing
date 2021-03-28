using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTemplateProjects;
using System;

[ExecuteInEditMode()]
public class Portal : MonoBehaviour
{
    public Transform target;
    public bool isActive = true;
    public Portal nextPortal;

    private Camera portalCamera;
    private static Portal activePortal;

    // Start is called before the first frame update
    void Start()
    {
        portalCamera = GameObject.FindGameObjectWithTag("portal camera").GetComponent<Camera>();
        if (isActive && activePortal == null) {
            activate();
        } else {
            deactivate();
        }
    }

    void Update()
    {
        if (!isActive) return;
        if (target != null && portalCamera != null) {
            Vector3 cameraLocalPosition = transform.InverseTransformPoint(Camera.main.transform.position);
            portalCamera.transform.position = target.TransformPoint(cameraLocalPosition);

            Vector3 cameraLocalForward = transform.InverseTransformDirection(Camera.main.transform.forward);
            Vector3 cameraLocalUp = transform.InverseTransformDirection(Camera.main.transform.up);
            portalCamera.transform.rotation = Quaternion.LookRotation(
                target.TransformDirection(cameraLocalForward),
                target.TransformDirection(cameraLocalUp));
        }
    }

    void OnTriggerEnter (Collider other) {
		if (other.tag == "MainCamera") {
            SimpleCameraController cameraController = Camera.main.GetComponent<SimpleCameraController>();
            if (cameraController != null && cameraController.enabled) {
                cameraController.TeleportTo(portalCamera.transform);
            } else {
                Camera.main.transform.position = portalCamera.transform.position;
                Camera.main.transform.rotation = portalCamera.transform.rotation;
                Camera.main.transform.localScale = portalCamera.transform.localScale;
            }
            if (nextPortal != null) nextPortal.activate();
        }
	}

    void OnDrawGizmos() {
        Gizmos.color = isActive ? Color.magenta : Color.gray;
        Vector3 offsetPoint = transform.TransformPoint(new Vector3(0,0,1));
        Vector3 targetOffsetPoint = target.TransformPoint(new Vector3(0,0,-1));
        Gizmos.DrawLine(transform.position, offsetPoint);
        Gizmos.DrawLine(offsetPoint, targetOffsetPoint);
        Gizmos.DrawLine(targetOffsetPoint, target.transform.position);

        if (nextPortal != null) {
            Gizmos.color = new Color(0,0.4f,0,1);
            Gizmos.DrawLine(target.transform.position, nextPortal.transform.position);
        }
    }

    public void activate() {
        if (activePortal != null) activePortal.deactivate();
        activePortal = this;
        isActive = true;
    }
    public void deactivate() {
        isActive = false;
        if (activePortal == this) activePortal = null;
    }
}

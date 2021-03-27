using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTemplateProjects;

public class PortalTeleporter : MonoBehaviour
{
    public bool shouldTeleport = false;
    public Camera otherPortalCamera;
    public Vector3 prevCameraPos;


    SimpleCameraController cameraController;

    // Start is called before the first frame update
    void Start() {
        cameraController = Camera.main.GetComponent<SimpleCameraController>();
    }

    // Update is called once per frame
    void Update() {
        Vector3 cameraMoveDirection = (Camera.main.transform.position - prevCameraPos).normalized;
        float movingTowards = Vector3.Dot(transform.forward, cameraMoveDirection);
        if (shouldTeleport) {
            if (cameraController != null) {
                cameraController.TeleportTo(otherPortalCamera.transform);
            }
            shouldTeleport = false;
        }
    }

    void OnTriggerEnter (Collider other) {
		if (other.tag == "MainCamera") shouldTeleport = true;
	}

	void OnTriggerExit (Collider other) {
		if (other.tag == "MainCamera") shouldTeleport = false;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTemplateProjects;

public class PortalTeleporter : MonoBehaviour
{
    public bool playerInTrigger = false;
    public Camera otherPortalCamera;
    float prevRelativeZ;


    SimpleCameraController cameraController;

    // Start is called before the first frame update
    void Start() {
        cameraController = Camera.main.GetComponent<SimpleCameraController>();
    }

    // Update is called once per frame
    void Update() {
        bool lookingAt = Vector3.Dot(Camera.main.transform.forward, transform.forward) > 0;
        float relativeZ = transform.InverseTransformPoint(Camera.main.transform.position).z;
        bool movingTowards = relativeZ-prevRelativeZ > 0;
        prevRelativeZ = relativeZ;

        if (playerInTrigger && movingTowards &&
            ((lookingAt && relativeZ < -0.4f) ||
            (!lookingAt && relativeZ > 0.4f))
        ) {
            print("teleport");
            if (cameraController != null) {
                cameraController.TeleportTo(otherPortalCamera.transform);
            }
            playerInTrigger = false;
        }
    }

    void OnTriggerEnter (Collider other) {
		if (other.tag == "MainCamera") playerInTrigger = true;
	}

	void OnTriggerExit (Collider other) {
		if (other.tag == "MainCamera") playerInTrigger = false;
	}

    void OnDrawGizmos() {
    }
}

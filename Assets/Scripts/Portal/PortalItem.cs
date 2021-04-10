// A "Portal Item": A twin of this object will be created and kept in the same relative position to
// the current portal's target as this object's relative position to the current portal itself


using System;
using UnityEngine;

public class PortalItem : MonoBehaviour
{
    [Tooltip("When a 'trigger' item touches the active portal, the next portal in the chain gets activated")]
    public bool isTrigger = false;
    [Tooltip("When a 'teleport' item touches the active portal, it teleports to it's twin's position")]
    public bool teleport = false;
    [Tooltip("List of component that should be disabled in this gameobject's 'twin'")]
    public string[] disableComponentsInTwin = { "AudioListener", "Collider" };

    private GameObject twin;
    private static Transform itemsParent;

    void Start()
    {
        // We'll group all of our created twins in a single gameobject, to keep the scene clean.
        // Create it if it's not there yet
        if (itemsParent == null) itemsParent = new GameObject("portalItems").transform;

        // Create the twin
        twin = GameObject.Instantiate(gameObject);
        twin.transform.parent = itemsParent;
        DisableComponentsInTwin();

        // Special case for the main camera: the twin of the main camera needs to render to a
        // renderTexture, of the same size as the screen, and assigned to the portal material.
        // This logic is managed by 'PortalTextureSetup'
        if (gameObject.tag == "MainCamera") {
            PortalTextureSetup.SetupCamera(twin.GetComponent<Camera>());
        }
    }

    /// Keep the twin in the same relative position to the portal target as the 'main' object's
    /// relative position to the portal
    void Update()
    {
        if (Portal.current == null) return;
        if (gameObject.isStatic &&
            Portal.current.gameObject.isStatic &&
            Portal.current.target.gameObject.isStatic) return;

        // Here, 'local' means 'local to the current portal'
        Vector3 sourceLocalPosition = Portal.current.transform.InverseTransformPoint(transform.position);
        twin.transform.position = Portal.current.target.TransformPoint(sourceLocalPosition);

        Vector3 sourceLocalForward = Portal.current.transform.InverseTransformDirection(transform.forward);
        Vector3 sourceLocalUp = Portal.current.transform.InverseTransformDirection(transform.up);
        twin.transform.rotation = Quaternion.LookRotation(
            Portal.current.target.TransformDirection(sourceLocalForward),
            Portal.current.target.TransformDirection(sourceLocalUp));
    }

    public void teleportToTwin() {
        transform.position = twin.transform.position;
        transform.rotation = twin.transform.rotation;
        transform.localScale = twin.transform.localScale;
    }

    void DisableComponentsInTwin() {
        // Always remove "portalItem" in twin, to avoid a recursive loop of portalItems
        Destroy(twin.GetComponent<PortalItem>());

        foreach (var componentName in disableComponentsInTwin)
        {
            var components = GetComponentsByName(twin, componentName);
            foreach (var component in components) Destroy(component);
        }
    }

    private static Component[] GetComponentsByName(GameObject obj, string componentName) {
        Type type = getTypeByName(componentName);
        if(type == null) return null;
        return obj.GetComponentsInChildren(type, false);
    }

    /// <summary>Wrapper around System.Type.GetType, that handles both 'native' unity objects and
    /// custom behaviours</summary>
    private static Type getTypeByName(string componentName) {
        Type componentType = Type.GetType(componentName);
        if (componentType == null) {
            String assemblyQualifiedName = String.Format("UnityEngine.{0}, UnityEngine", componentName);
            componentType = Type.GetType(assemblyQualifiedName);
        }
        return componentType;
    }
}

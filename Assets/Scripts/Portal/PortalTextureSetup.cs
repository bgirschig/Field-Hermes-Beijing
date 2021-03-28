using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTextureSetup : MonoBehaviour
{
    public Material portalMaterial;
    private Vector2 prevScreenSize;
    private Camera portalCamera;

    // Start is called before the first frame update
    void Start()
    {
        portalCamera = GameObject.FindGameObjectWithTag("portal camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (hasScreenSizeChanged()) UpdateTextureSize();
    }

    bool hasScreenSizeChanged() {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
		if (screenSize == prevScreenSize) return false;
		prevScreenSize = new Vector2(Screen.width, Screen.height);
        return true;
    }

    void UpdateTextureSize() {
        if (portalCamera == null) return;
        if (portalMaterial == null) return;

        if (portalCamera.targetTexture != null) portalCamera.targetTexture.Release();
        portalCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        portalMaterial.mainTexture = portalCamera.targetTexture;
    }
}

using UnityEngine;

public class PortalTextureSetup : MonoBehaviour
{
    public Material portalMaterial;

    private static Material staticPortalMaterial;
    private static Vector2 prevScreenSize;
    private static Camera portalCamera;
    private static bool initialized;

    void Start() {
        Debug.Assert(
            initialized == false,
            "Found multiple instances of PortalTextureSetup. Make sure there is only one");

        staticPortalMaterial = portalMaterial;
        initialized = true;
    }

    void Update()
    {
        if (screenSizeHasChanged()) UpdateTextureSize();
    }

    static bool screenSizeHasChanged() {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
		if (screenSize == prevScreenSize) return false;
		prevScreenSize = new Vector2(Screen.width, Screen.height);
        return true;
    }

    static void UpdateTextureSize() {
        if (portalCamera == null) return;
        if (staticPortalMaterial == null) return;

        if (portalCamera.targetTexture != null) portalCamera.targetTexture.Release();
        portalCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        staticPortalMaterial.mainTexture = portalCamera.targetTexture;
    }

    public static void SetupCamera(Camera cam) {
        if (cam == null) return;
        portalCamera = cam;
        UpdateTextureSize();
    }
}

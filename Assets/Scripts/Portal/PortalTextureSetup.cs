/// Update the portal texture, so that the screen space texture is displayed correctly
// ! This component assumes (see getCurrentDisplay):
// - We're fullscreen
// - The main camera is rendering to the screen we need to match

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

    static Display getCurrentDisplay() {
        return Display.displays[Camera.main.targetDisplay % Display.displays.Length];
    }

    static bool screenSizeHasChanged() {
        Display display = getCurrentDisplay();
        Vector2 screenSize = new Vector2(display.systemWidth, display.systemHeight);
		if (screenSize == prevScreenSize) return false;
		prevScreenSize = screenSize;
        return true;
    }

    static void UpdateTextureSize() {
        if (portalCamera == null) return;
        if (staticPortalMaterial == null) return;

        if (portalCamera.targetTexture != null) portalCamera.targetTexture.Release();
        Display display = getCurrentDisplay();
        portalCamera.targetTexture = new RenderTexture(display.systemWidth, display.systemHeight, 24);
        staticPortalMaterial.mainTexture = portalCamera.targetTexture;
    }

    public static void SetupCamera(Camera cam) {
        if (cam == null) return;
        portalCamera = cam;
        UpdateTextureSize();
    }
}

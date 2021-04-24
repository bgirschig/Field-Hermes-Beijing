/// Handles swapping the culling mask on different elements (camera, light, etc...)
// ! This component should probably not be re-used in another project because it makes
// bug assumptions about how the scene is structured:
// - must be using a portal chain
// - teleportations must happen in the correct order
// - the "Update culling" is not idempotent (calling it twice will break stuff)

using UnityEngine;

public class PortalCullingSelector : MonoBehaviour
{
    public PortalItem portalItem;

    private Light _light;
    private Camera _camera;
    private Light _twinLight;
    private Camera _twinCamera;

    // Start is called before the first frame update
    void Start()
    {
        _light = GetComponent<Light>();
        _camera = GetComponent<Camera>();
        if (_light != null) _twinLight = PortalItem.findTwin<Light>(_light);
        if (_camera != null) _twinCamera = PortalItem.findTwin<Camera>(_camera);

        portalItem.onTeleport.AddListener(OnTeleport);
        UpdateCulling();
    }

    void UpdateCulling() {
        LayerMask newMask = Portal.current.target.GetComponent<PortalTarget>().cullingMask;
        if (_light != null) {
            _light.cullingMask = _twinLight.cullingMask;
            _twinLight.cullingMask = newMask;
        }
        if (_camera != null) {
            _camera.cullingMask = _twinCamera.cullingMask;
            _twinCamera.cullingMask = newMask;
        }
    }

    void OnTeleport(Vector3 positionOffset) {
        UpdateCulling();
    }
}

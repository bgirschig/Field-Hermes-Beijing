// Creates a render texture for this camera, and makes the camera render to it

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class CameraRenderTexture : MonoBehaviour
{
    [Tooltip("[optionnal] The camera's render texture will be set as the texture for this rawImage")]
    public RawImage viewer;
    private Camera _camera;
    private int currentWidth;
    private int currentHeight;

    void Update()
    {
        // Check for updates in screen size, to redo the setup with the correct size
        if (currentWidth != Screen.currentResolution.width || currentHeight != Screen.currentResolution.height) {
            currentWidth = Screen.currentResolution.width;
            currentHeight = Screen.currentResolution.height;
            SetupRT();
        }
    }

    void SetupRT() {
        _camera = GetComponent<Camera>();
        _camera.depthTextureMode = DepthTextureMode.DepthNormals;

        // Remove any existing render texture
        if (_camera.targetTexture != null) {
            RenderTexture temp = _camera.targetTexture;
            _camera.targetTexture = null;
            DestroyImmediate(temp);
        }

        // create new render texture
        _camera.targetTexture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 16);
        _camera.targetTexture.filterMode = FilterMode.Bilinear;

        // assign texture to viewer
        if (viewer) {
            viewer.enabled = true;
            viewer.texture = _camera.targetTexture;
        }
    }
}
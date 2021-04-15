using UnityEngine;

public class FullscreenControl : MonoBehaviour
{
    public KeyCode key;
    [Tooltip("prefered ratio for windowed mode. set to -1 to keep")]
    public float ratio = -1;

    void Update()
    {
        if (Input.GetKeyUp(key)) {
            var screenResolution = Screen.resolutions[Screen.resolutions.Length - 1];
            float width = screenResolution.width;
            float height = screenResolution.height;
            if (Screen.fullScreen) {
                float screenRatio = width / height;
                if (screenRatio > ratio) width = height * ratio;
                else height = width / ratio;
                Screen.SetResolution((int)(width*0.8f), (int)(height*0.8f), FullScreenMode.Windowed, screenResolution.refreshRate);
            } else {
                Screen.SetResolution((int)width, (int)height, FullScreenMode.ExclusiveFullScreen, screenResolution.refreshRate);
            }
        }        
    }
}

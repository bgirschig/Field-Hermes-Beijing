/// Manages the displays / screen windows
// - Activates all displays (https://docs.unity3d.com/Manual/MultiDisplay.html)
// - Enters full screen mode (multi-display doesn't work with windowed game)
// - Moves content (DisplayGroupItems) to their desired location
//   This ensures each "DisplayGroup" is displayed separately. If there isn't
//   enough displays to show them all, the contents won't be visible by default
// - Exposes a shortcut key to cycle DisplayGroups on the available screens (tab)

using UnityEngine;
using UnityEngine.EventSystems;

public class DisplayManager : MonoBehaviour
{
    private DisplayGroupItem[] displayGroupItems;
    int currentOffset = 0;
    int displayGroupCount = 0;

    void Start ()
    {
        Debug.Log ("displays connected: " + Display.displays.Length);
    
        // activate displays
        for (int i = 1; i < Display.displays.Length; i++) Display.displays[i].Activate();

        // figure out how many display groups we have
        displayGroupItems = GameObject.FindObjectsOfType<DisplayGroupItem>();
        foreach (var item in displayGroupItems)
        {
            if (item.displayGroup+1 > displayGroupCount) displayGroupCount = item.displayGroup + 1;
        }

        // go fullscreen (multiple displays don't work with multiple displays)
        var screenResolution = Screen.resolutions[Screen.resolutions.Length - 1];
        float width = screenResolution.width;
        float height = screenResolution.height;
        Screen.SetResolution((int)width, (int)height, FullScreenMode.Windowed, screenResolution.refreshRate);
    }

    void Update() {
        // Cycle display groups on available displays
        if (Input.GetKeyUp(KeyCode.Tab) && EventSystem.current.currentSelectedGameObject == null) {
            currentOffset = (currentOffset + 1) % displayGroupCount;
            foreach (var item in displayGroupItems)
            {
                item.setTargetDisplay((item.displayGroup + currentOffset) % displayGroupCount);
            }
        }
    }
}

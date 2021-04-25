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
        foreach (var display in Display.displays) {
            display.Activate();
            display.SetRenderingResolution(display.systemWidth, display.systemHeight);
        }

        // The unity doc is not entirely clear about which display Screen.SetResolution applies to,
        // but using the first display from Display.displays seems to work in our case.
        // TODO: figure out how to do this properly (without assuming Screen.setResolution targets the first display)
        Screen.SetResolution(
            Display.displays[0].systemWidth,
            Display.displays[0].systemHeight,
            FullScreenMode.Windowed,
            60);

        // figure out how many display groups we have
        displayGroupItems = GameObject.FindObjectsOfType<DisplayGroupItem>();
        foreach (var item in displayGroupItems)
        {
            if (item.displayGroup+1 > displayGroupCount) displayGroupCount = item.displayGroup + 1;
        }
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

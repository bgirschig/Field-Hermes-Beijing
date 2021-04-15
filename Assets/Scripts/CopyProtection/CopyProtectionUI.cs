using UnityEngine;
using UnityEngine.UI;

public class CopyProtectionUI : MonoBehaviour
{
    Text text;
    string textContent;

    void Start() {
        text = GetComponentInChildren<Text>();
    }

    void Update()
    {
        bool metaPressed = Input.GetKey(KeyCode.LeftCommand) ||
            Input.GetKey(KeyCode.RightCommand) || 
            Input.GetKey(KeyCode.LeftControl) || 
            Input.GetKey(KeyCode.RightControl);
        if (Input.GetKeyUp(KeyCode.C) && metaPressed) {
            GUIUtility.systemCopyBuffer = textContent;
        }
    }

    public void showViolation(CopyProtection.Violation violation) {
        gameObject.SetActive(true);
        text = GetComponentInChildren<Text>();
        textContent = $"{violation.message}.\n"+
            $"Please contact {Application.companyName} with the following details to resolve the issue:\n"+
            "\n"+
            $"device ID: {SystemInfo.deviceUniqueIdentifier}\n"+
            $"device name: {SystemInfo.deviceName}\n";
        text.text = $"{textContent}\nPress CTRL/CMD + C to copy this message";
    }
}

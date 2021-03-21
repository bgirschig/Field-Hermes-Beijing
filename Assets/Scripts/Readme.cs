using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Allows swapping between the child object. Useful to compare objects

public class Readme : MonoBehaviour
{
    [TextArea(5, 40)]
    public string readme;
    public bool showInScene = true;
    public Vector2 pos = new Vector2(20, 20);

    void OnGUI() {
        if (!showInScene) return;
        var style = new GUIStyle();
        style.fontSize = Screen.width/20;
        style.normal.textColor = new Color(255,255,255);
        GUI.TextArea(new Rect(pos.x, pos.y, 100, 100), readme, style);
    }
}

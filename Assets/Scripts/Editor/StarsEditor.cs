// Custom editor for the stars script (Allows fancy stuff like MinMaxSliders)

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Stars))]
[CanEditMultipleObjects]
public class StarsEditor : Editor 
{
    SerializedProperty starCount;
    // 
    SerializedProperty minRotation;
    SerializedProperty maxRotation;
    // 
    SerializedProperty starSize;
    SerializedProperty starCompensatePerpective;
    // 
    SerializedProperty clipXMin;
    SerializedProperty clipXMax;
    SerializedProperty clipYMin;
    SerializedProperty clipYMax;
    SerializedProperty clipZMin;
    SerializedProperty clipZMax;

    void OnEnable()
    {
        starCount = serializedObject.FindProperty("starCount");
        //
        minRotation = serializedObject.FindProperty("minRotation");
        maxRotation = serializedObject.FindProperty("maxRotation");
        //
        starSize = serializedObject.FindProperty("starSize");
        starCompensatePerpective = serializedObject.FindProperty("compensatePerspective");
        //
        clipXMin = serializedObject.FindProperty("clipXMin");
        clipXMax = serializedObject.FindProperty("clipXMax");
        clipYMin = serializedObject.FindProperty("clipYMin");
        clipYMax = serializedObject.FindProperty("clipYMax");
        clipZMin = serializedObject.FindProperty("clipZMin");
        clipZMax = serializedObject.FindProperty("clipZMax");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(starCount);
        EditorGUILayout.PropertyField(starSize);
        EditorGUILayout.PropertyField(starCompensatePerpective);
        MinMaxSlider(minRotation, maxRotation, 0, 180, "Star rotations");

        EditorGUILayout.LabelField("Clip Space");
        EditorGUI.indentLevel++;
        EditorGUILayout.HelpBox(
            "By default, stars will be generated across the full width, height, and depth of the camera 'range'\n" +
            "You can restrict that with these sliders (eg. when a floor is hiding the lower half)", MessageType.Info);
        MinMaxSlider(clipXMin, clipXMax, 0, 1, "Clip X");
        MinMaxSlider(clipYMin, clipYMax, 0, 1, "Clip Y");
        MinMaxSlider(clipZMin, clipZMax, 0, 1, "Clip Z");
        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();
    }

    void MinMaxSlider(SerializedProperty minProperty, SerializedProperty maxProperty, float minValue, float maxValue, string name, string tooltip="")
    {
        Rect position = EditorGUILayout.GetControlRect();
        
        var label = EditorGUI.BeginProperty(position, new GUIContent(name), minProperty);
        label.tooltip = tooltip;

        EditorGUI.BeginChangeCheck();
        float newMin = minProperty.floatValue;
        float newMax = maxProperty.floatValue;
        EditorGUI.MinMaxSlider(position, label, ref newMin, ref newMax, minValue, maxValue);
        // Only assign the value if it was changed by the user.
        // Otherwise a value will be assigned to all objects, even when the user doesn't touch the control.
        if (EditorGUI.EndChangeCheck())
        {
            minProperty.floatValue = newMin;
            maxProperty.floatValue = newMax;
        }
        EditorGUI.EndProperty();
    }
}
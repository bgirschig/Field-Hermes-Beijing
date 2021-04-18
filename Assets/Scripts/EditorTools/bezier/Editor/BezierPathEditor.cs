/// Custom editor for the bezier path component

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierPath))]
public class BezierPathEditor : Editor
{
    BezierPath bezierPath;
    Vector3 pos;

    SerializedProperty resolution;
    SerializedProperty spacing;

    int activeAnchor = -1;

    void OnEnable() {
        bezierPath = (BezierPath)target;

        resolution = serializedObject.FindProperty("bezier.resolution");
        spacing = serializedObject.FindProperty("bezier.spacing");
    }

    void OnSceneGUI() {
        Input();
        Draw();
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(resolution);
        EditorGUILayout.PropertyField(spacing);
        serializedObject.ApplyModifiedProperties();

        bezierPath.bezier.Recompute();
    }

    void deleteActiveAnchor() {
        Undo.RecordObject(bezierPath, "Delete point");
        bezierPath.bezier.deleteAnchor(activeAnchor);
        activeAnchor = -1;
        Repaint();
        EditorApplication.QueuePlayerLoopUpdate();
    }

    void Input() {
        Event guiEvent = Event.current;
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift) {
            // TODO: It seems there should be a way to simplify this computation of the next point's position

            Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
            Transform currentCamera = SceneView.lastActiveSceneView.camera.transform;
            
            // local space
            Vector3 lastPointPos = bezierPath.bezier.points[bezierPath.bezier.points.Count-1];
            // world space
            lastPointPos = bezierPath.transform.TransformPoint(lastPointPos);
            // camera space
            lastPointPos = currentCamera.InverseTransformPoint(lastPointPos);
            // distance to camera (orthographic)
            float distance = lastPointPos.z;

            Vector3 direction = currentCamera.InverseTransformDirection(mouseRay.direction);
            float factor = lastPointPos.z / direction.z;
            Vector3 newPoint = direction * factor;
            newPoint = currentCamera.TransformPoint(newPoint);

            Undo.RecordObject(bezierPath, $"Add bezier point");
            bezierPath.bezier.addSegment(bezierPath.transform.InverseTransformPoint(newPoint));
            EditorApplication.QueuePlayerLoopUpdate();
        }

        if (activeAnchor>=0 &&
            guiEvent.isKey &&
            guiEvent.type.Equals(EventType.KeyDown) &&
            (guiEvent.keyCode == KeyCode.Delete || guiEvent.keyCode == KeyCode.Backspace)) {
                guiEvent.Use();
                deleteActiveAnchor();
        }
    }

    void Draw() {
        Bezier bezier = bezierPath.bezier;
        Handles.color = Color.red;

        for (int i = 0; i < bezierPath.bezier.points.Count; i+=3) {
            Vector3 handlePos = bezierPath.transform.TransformPoint(bezier[i]);
            float size = HandleUtility.GetHandleSize(handlePos);
            if (activeAnchor == i) {
                MakeHandle(i);
                if (i > 0) {
                    Handles.DrawLine(handlePos, bezierPath.transform.TransformPoint(bezier[i-1]));
                    MakeHandle(i-1);
                }
                if (i < bezier.points.Count - 1) {
                    Handles.DrawLine(handlePos, bezierPath.transform.TransformPoint(bezier[i+1]));
                    MakeHandle(i+1);
                }
            } else {
                Quaternion rotation = Quaternion.LookRotation(Camera.current.transform.forward, Camera.current.transform.up);
                bool clicked = Handles.Button(handlePos, rotation, size * 0.1f, size * 0.2f, Handles.ConeHandleCap);
                if (clicked) {
                    activeAnchor = i;
                    Repaint ();
                }
            }
        }
    }

    void MakeHandle(int index) {
        Vector3 position = bezierPath.transform.TransformPoint(bezierPath.bezier[index]);
        float size = HandleUtility.GetHandleSize(position) * 0.1f;
        
        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.FreeMoveHandle(
            position, Quaternion.identity, size, Vector3.zero, Handles.ConeHandleCap);
        bool hasChanged = EditorGUI.EndChangeCheck();

        if (hasChanged) {
            Undo.RecordObject(bezierPath, "Move point");
            bezierPath.movePoint(index, bezierPath.transform.InverseTransformPoint(newPosition));
            EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}

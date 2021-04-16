/// Custom editor for the bezier path component

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierPath))]
public class BezierPathEditor : Editor
{
    BezierPath bezierPath;
    Vector3 pos;

    void OnEnable() {
        bezierPath = (BezierPath)target;
    }

    void OnSceneGUI() {
        Input();
        Draw();
    }

    void Input() {
        Event guiEvent = Event.current;
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        Vector3 mousePos = mouseRay.origin + mouseRay.direction*5;
        // Vector3 mousePosInCamera = SceneView.lastActiveSceneView.camera.transform.InverseTransformPoint(mousePos);
        // Debug.Log(mousePosInCamera.z);

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift) {
            Undo.RecordObject(bezierPath, "add point");
            bezierPath.bezier.addSegment(bezierPath.transform.InverseTransformPoint(mousePos));
            EditorApplication.QueuePlayerLoopUpdate();
        }
    }

    void Draw() {
        Bezier bezier = bezierPath.bezier;
        Handles.color = Color.red;

        for (int i = 0; i < bezier.numSegment; i++) {
            Vector3[] points = bezier.GetPointInSegment(i);
            Handles.DrawLine(bezierPath.transform.TransformPoint(points[0]), bezierPath.transform.TransformPoint(points[1]));
            Handles.DrawLine(bezierPath.transform.TransformPoint(points[2]), bezierPath.transform.TransformPoint(points[3]));
        }

        for (int i = 0; i < bezier.numPoints; i++) {
            float size = HandleUtility.GetHandleSize(bezierPath.transform.TransformPoint(bezier[i])) * 0.1f;
            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = Handles.FreeMoveHandle(bezierPath.transform.TransformPoint(bezier[i]), Quaternion.identity, size, Vector3.zero, Handles.ConeHandleCap);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(bezierPath, "Move point");
                bezierPath.movePoint(i, bezierPath.transform.InverseTransformPoint(newPosition));
                EditorApplication.QueuePlayerLoopUpdate();
            }
        }
    }
}

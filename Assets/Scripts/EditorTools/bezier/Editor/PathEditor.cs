using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    PathCreator creator;
    BezierPath path;
    Vector3 pos;

    void OnEnable() {
        creator = (PathCreator)target;
        if (creator.path == null) creator.CreatePath();
        path = creator.path;
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
            Undo.RecordObject(creator, "add point");
            path.addSegment(creator.transform.InverseTransformPoint(mousePos));
        }
    }

    void Draw() {
        Handles.color = Color.red;

        for (int i = 0; i < path.numSegment; i++) {
            Vector3[] points = path.GetPointInSegment(i);
            Handles.DrawBezier(
                creator.transform.TransformPoint(points[0]),
                creator.transform.TransformPoint(points[3]),
                creator.transform.TransformPoint(points[1]),
                creator.transform.TransformPoint(points[2]),
                Color.green, null, 2);
            Handles.DrawLine(creator.transform.TransformPoint(points[0]), creator.transform.TransformPoint(points[1]));
            Handles.DrawLine(creator.transform.TransformPoint(points[2]), creator.transform.TransformPoint(points[3]));
        }

        for (int i = 0; i < path.numPoints; i++) {
            float size = HandleUtility.GetHandleSize(creator.transform.TransformPoint(path[i])) * 0.1f;
            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = Handles.FreeMoveHandle(creator.transform.TransformPoint(path[i]), Quaternion.identity, size, Vector3.zero, Handles.ConeHandleCap);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(creator, "Move point");
                path.movePoint(i, creator.transform.InverseTransformPoint(newPosition));
            }
        }
    }
}

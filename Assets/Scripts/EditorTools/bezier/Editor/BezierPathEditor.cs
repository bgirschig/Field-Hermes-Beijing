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

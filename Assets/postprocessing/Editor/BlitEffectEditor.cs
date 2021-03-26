using UnityEditor;
using UnityEngine;
using System.Collections;

// Custom Editor using SerializedProperties.
// Automatic handling of multi-object editing, undo, and Prefab overrides.
[CustomEditor(typeof(BlitEffect))]
public class BlitEffectEditor : Editor
{
    private BlitEffect _blitEffect;
    private MaterialEditor _materialEditor; 

    void OnEnable()
    {
        _blitEffect = (BlitEffect)target;

        // Create an instance of the default MaterialEditor
        if (_blitEffect.effectMaterial != null) {
            _materialEditor = (MaterialEditor)CreateEditor (_blitEffect.effectMaterial);
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck ();

        DrawDefaultInspector();
        
        if (EditorGUI.EndChangeCheck ()) {
            serializedObject.ApplyModifiedProperties (); 
 
            if (_materialEditor != null) {
                DestroyImmediate (_materialEditor);
            }
            if (_blitEffect.effectMaterial != null) {
                _materialEditor = (MaterialEditor)CreateEditor (_blitEffect.effectMaterial);
            }
        }

        if (_materialEditor != null) {
            _materialEditor.DrawHeader();
            
            bool isDefaultMaterial = !AssetDatabase.GetAssetPath (_blitEffect.effectMaterial).StartsWith ("Assets");
            using (new EditorGUI.DisabledGroupScope(isDefaultMaterial)) {
                _materialEditor.OnInspectorGUI (); 
            }
        }
    }

    void OnDisable ()
    {
        if (_materialEditor != null) {
            // Free the memory used by default MaterialEditor
            DestroyImmediate (_materialEditor);
        }
    }
}
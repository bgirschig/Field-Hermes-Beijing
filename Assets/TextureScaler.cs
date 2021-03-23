using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TextureScaler : MonoBehaviour
{
    public float scale = 1;
    Renderer objectRenderer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

   #if UNITY_EDITOR
    void Update() {
        updateScale();
    }
    #endif

    void updateScale() {
        if (objectRenderer == null) objectRenderer = GetComponent<Renderer>();

        objectRenderer.sharedMaterial.mainTextureScale = new Vector2(
            10 * transform.localScale.x / scale,
            10 * transform.localScale.y / scale
        );
    }
}

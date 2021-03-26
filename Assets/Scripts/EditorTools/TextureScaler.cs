// Scales the attached renderer's main texture to compensate the GameObject's localscale

using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
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

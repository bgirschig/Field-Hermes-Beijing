using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class BlitEffect : MonoBehaviour {
    public Material material;
    
    // Postprocess the image
    void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        if (!material) {
            Graphics.Blit(source, destination);
            return;
        }

        Graphics.Blit (source, destination, material);
    }
}
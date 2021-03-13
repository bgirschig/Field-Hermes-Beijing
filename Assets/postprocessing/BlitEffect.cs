using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class BlitEffect : MonoBehaviour {
    public Shader shader; 

    public Material material;
    
    // Postprocess the image
    void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        if (!shader) {
            Graphics.Blit(source, destination);
            return;
        }
        if (!material || material.shader != shader) {
            material = new Material(shader);
        }

        Graphics.Blit (source, destination, material);
    }
}
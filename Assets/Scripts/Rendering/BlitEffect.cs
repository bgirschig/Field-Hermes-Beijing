// Applies an effect to the output of a camera

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class BlitEffect : MonoBehaviour {
    private const int LAYERS_EVERYTHING = ~0;
    private const int LAYERS_NONE = 0;

    [Tooltip("The material to be used for this effect")]
    public Material effectMaterial;

    [Tooltip("Make some additionnal textures available to the effectShader, eg. _CameraDepthTexture  or _CameraDepthNormalsTexture")]
    public DepthTextureMode depthTextureMode;

    [Tooltip("Optionnal layer mask to limit on What objects the effect is applied. Select 'Everything' for better performance")]
    public LayerMask layerMask = LAYERS_EVERYTHING;

    [Tooltip("What output to show. This is mostly for debugging")]
    public ViewSelector view = ViewSelector.Output;


    // Used to draw a "stencil" with all the objects that should be affected (white) or not be affected (black)
    private Material silhouetteBlack;
    private Material silhouetteWhite;
    // A list of all renderers in the scene: we'll need to render each to construct our stencil.
    // We can't simply render the affected ones, since affected and unaffected items need to occlude each other)
    private RendererInfo[] renderables;
    private Camera cam;

    void Start() {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = depthTextureMode;

        UpdateRenderablesList();
        CreateSilhouetteMaterials();
    }

    // Postprocess the image
    void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        if (!effectMaterial || layerMask == LAYERS_NONE || view == ViewSelector.Input) {
            Graphics.Blit(source, destination);
        } else {
            UpdateSelectionBuffer(source, destination);
            if (view == ViewSelector.Output) {
                Graphics.Blit(source, destination, effectMaterial);
            }
        }
    }

    void UpdateSelectionBuffer(RenderTexture source, RenderTexture destination) {
        var commands = new CommandBuffer();
        int selectionBuffer = Shader.PropertyToID("_SelectionBuffer");
        commands.GetTemporaryRT(selectionBuffer, source.descriptor);

        commands.SetRenderTarget(selectionBuffer);

        if (layerMask != LAYERS_EVERYTHING) {
            DrawSilhouettes(commands);
        } else {
            // If layerMask is set to 'Everything', we don't need to render individual objects.
            // instead, we simply 'clear' the _SelectionBuffer texture to white
            commands.ClearRenderTarget(true, true, Color.white);
        }
        
        if (view == ViewSelector.Mask) {
            commands.Blit (selectionBuffer, destination);
        }

        commands.ReleaseTemporaryRT(selectionBuffer);
        //execute and clean up commandbuffer itself
        Graphics.ExecuteCommandBuffer(commands);
        commands.Dispose();
    }

    void DrawSilhouettes(CommandBuffer commands) {
        if (silhouetteWhite==null || silhouetteWhite == null) CreateSilhouetteMaterials();
        commands.ClearRenderTarget(true, true, Color.clear);
        // TODO: calling this every frame is not very efficient but also not awful
        // measured execution time on our final scene: 0.058ms, twice (because it runs on each camera)
        UpdateRenderablesList();
        foreach (RendererInfo rendererInfo in renderables) {
            if (!rendererInfo.renderer.gameObject.activeInHierarchy) continue;
            if (!layerInMask(rendererInfo.layer, cam.cullingMask)) continue; // skip culled objects
            var hasLayer = ((layerMask.value & (1 << rendererInfo.layer)) > 0);
            var mat = layerInMask(rendererInfo.layer, layerMask) ? silhouetteWhite : silhouetteBlack;
            // Text renderers don't have a meshFilter. They also don't work well  
            if (rendererInfo.subMeshCount == 0) {
                // TODO: We shouldn't be using the mesh's 'normal' material to render to our stencil.
                // This only works when the material is set to write unlit white.
                // Possible solutions:
                //   - A custom render pass that only this code uses, that does the equivalent of the silhouette materials.
                //   - A blit call after the DrawRenders, that ignores any color information
                // commands.DrawRenderer(rendererInfo.renderer, rendererInfo.renderer.sharedMaterial);
            }
            for (int subMeshIndex = 0; subMeshIndex < rendererInfo.subMeshCount; subMeshIndex++)
            {
                commands.DrawRenderer(rendererInfo.renderer, mat, submeshIndex:subMeshIndex);
            }
        }
    }

    void UpdateRenderablesList() {
        var renderers = GameObject.FindObjectsOfType<Renderer>();
        renderables = new RendererInfo[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            renderables[i].renderer = renderers[i];
            MeshFilter meshFilter = renderers[i].GetComponent<MeshFilter>();
            if (meshFilter != null) {
                renderables[i].subMeshCount = meshFilter.sharedMesh.subMeshCount;
            } else {
                renderables[i].subMeshCount = 0;
            }
            renderables[i].layer = renderers[i].gameObject.layer;
        }
    }

    void CreateSilhouetteMaterials() {
        var shader = Shader.Find("Hidden/silhouette");
        silhouetteBlack = new Material(shader);
        silhouetteBlack.SetColor("_mainColor", Color.black);
        silhouetteWhite = new Material(shader);
        silhouetteWhite.SetColor("_mainColor", Color.white);
    }

    void Reset() {
        depthTextureMode = cam.depthTextureMode;
    }

    void OnValidate() {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = depthTextureMode;
    }

    static bool layerInMask(int layer, LayerMask mask) {
        return ((mask.value & (1 << layer)) > 0);
    }
    
    public struct RendererInfo {
        public Renderer renderer;
        public int subMeshCount;
        public int layer;
    }

    public enum ViewSelector {
        Input,
        Mask,
        Output,
    }
}
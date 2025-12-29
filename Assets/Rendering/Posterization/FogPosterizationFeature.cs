using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using Effects;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEditor.Rendering;

public class FogPosterizationFeature : ScriptableRendererFeature
{

    class FogPosterizationPass : ScriptableRenderPass
    {
        const string PassName = "SpeedLinesPass";
        Material material;
        FogPosterization posterization;

        public FogPosterizationPass(Material material)
        {
            this.material = material;
            requiresIntermediateTexture = true;
        }

        private void UpdatePosterizationSettings()
        {
            if (material == null) return;

            var stack = VolumeManager.instance.stack;
            this.posterization = stack.GetComponent<FogPosterization>();
            if (this.posterization == null) { return; }
            if (!this.posterization.IsActive()) { return; }

            material.SetFloat("_Min", posterization.MinClamping.value);
            material.SetFloat("_Max", posterization.MaxClamping.value);
            material.SetFloat("_Steps", posterization.Steps.value);
            material.SetVector("_Remaping", posterization.Remaping.value);
            material.SetInt("_Clamping", posterization.Clamping.value ? 1 : 0);
        }


        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var cameraData = frameData.Get<UniversalCameraData>();
            UpdatePosterizationSettings();

            var resourceData = frameData.Get<UniversalResourceData>();

            var source = resourceData.activeColorTexture;

            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = $"CameraColor-{PassName}";
            destinationDesc.clearBuffer = false;

            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

            destinationDesc.filterMode = FilterMode.Bilinear;


            RenderGraphUtils.BlitMaterialParameters pass = new(source, destination, material, 0);

            renderGraph.AddBlitPass(pass);

            resourceData.cameraColor = destination;
        }
    }

    FogPosterizationPass fPass;
    Material material;
    public Shader FogPosterizationShader;
    public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;


    public override void Create()
    {
        material = new Material(FogPosterizationShader);
        fPass = new FogPosterizationPass(material);

        fPass.renderPassEvent = injectionPoint;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
        if (fPass == null)
        {
            return;
        }

        renderer.EnqueuePass(fPass);
    }

    protected override void Dispose(bool disposing)
    {
        if (Application.isPlaying)
        {
            Destroy(material);
        }
        else
        {
            DestroyImmediate(material);
        }
    }

}

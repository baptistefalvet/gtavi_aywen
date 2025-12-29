using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using Effects;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEditor.Rendering;

public class SpeedLinesFeature : ScriptableRendererFeature
{

    class SpeedLinesPass : ScriptableRenderPass
    {
        const string PassName = "SpeedLinesPass";
        Material material;
        SpeedLines speedLines;

        public SpeedLinesPass(Material material)
        {
            this.material = material;
            requiresIntermediateTexture = true;
        }

        private void UpdateSpeedLinesSettings()
        {
            if (material == null) return;

            var stack = VolumeManager.instance.stack;
            this.speedLines = stack.GetComponent<SpeedLines>();
            if (this.speedLines == null) { return; }
            if (!this.speedLines.IsActive()) { return; }

            material.SetColor("_LinesColor", speedLines.LinesColor.value);
            material.SetFloat("_LinesEdges", speedLines.LinesEdges.value);
            material.SetFloat("_EffectRaduis", speedLines.EffectRaduis.value);
            material.SetFloat("_Speed", speedLines.EffectSpeed.value);
            material.SetVector("_RedOffset", speedLines.RedOffset.value);
            material.SetVector("_GreenOffset", speedLines.GreenOffset.value);
            material.SetVector("_BlueOffset", speedLines.BlueOffset.value);
        }


        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var cameraData = frameData.Get<UniversalCameraData>();
            UpdateSpeedLinesSettings();

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

    SpeedLinesPass slPass;
    Material material;
    public Shader SpeedLinesShader;
    public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;


    public override void Create()
    {
        material = new Material(SpeedLinesShader);
        slPass = new SpeedLinesPass(material);

        slPass.renderPassEvent = injectionPoint;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
        if (slPass == null)
        {
            return;
        }

        renderer.EnqueuePass(slPass);
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

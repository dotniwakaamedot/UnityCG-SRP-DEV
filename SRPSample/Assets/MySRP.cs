using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

// 1. 継承したクラスの定義
[ExecuteInEditMode]
public class MySRPAsset : RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline()
    {
        return new MySRPInstance();
    }

}

public class MySRPInstance : RenderPipeline
{
    static readonly ShaderTagId BasePassName = new ShaderTagId("BasicPass");
    static readonly List<ShaderTagId> k_ShaderTags = new List<ShaderTagId>() { BasePassName, };

    // 描画コマンドバッファ
    private CommandBuffer commandBuffer;


    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        if(commandBuffer == null)
        {
            commandBuffer = new CommandBuffer();
        }

        foreach(var camera in cameras)
        {
            // カリングの設定
            ScriptableCullingParameters cullParam = new ScriptableCullingParameters();
            if( !camera.TryGetCullingParameters( out cullParam ) )
                continue;
            var cullResults = context.Cull(ref cullParam);

            // カメラの設定
            context.SetupCameraProperties(camera);

            // ディレクショナルライトの設定
            SetUpDirectionalLightParam(cullResults.visibleLights);


            // ---- 画面クリア用の処理です -----//
            // 描画コマンドのクリアします
            commandBuffer.Clear();
            // 画面のクリアコマンド
            commandBuffer.ClearRenderTarget(true, true, Color.black,1.0f);
            // 描画コマンドを実行します
            context.ExecuteCommandBuffer(commandBuffer);

           

            // 描画処理の記載
            SortingSettings sortSettings = new SortingSettings(camera);
            sortSettings.criteria = SortingCriteria.CommonOpaque;

            // フィルターの設定
            FilteringSettings filterSettings = new FilteringSettings();
            filterSettings.renderQueueRange = RenderQueueRange.opaque;
            filterSettings.layerMask = -1;
            filterSettings.renderingLayerMask = 0xFFFFFFFF;
            filterSettings.sortingLayerRange = SortingLayerRange.all;

            // 描画設定
            DrawingSettings drawSettings = new DrawingSettings(BasePassName, sortSettings);

            // 描画
            context.DrawRenderers(cullResults , ref drawSettings, ref filterSettings);

        }


        // コマンドのサブミット
        context.Submit();


        // ImageEffectの記載
    }

    // Directional Lightの内容をShaderに反映させます
    private void SetUpDirectionalLightParam(NativeArray<VisibleLight> visibleLights)
    {
        if( visibleLights.Length <= 0 ){
            return;
        }
        foreach( var visibleLight in visibleLights)
        {
            if (visibleLight.lightType == LightType.Directional)
            {
                Vector4 dir = -visibleLight.localToWorldMatrix.GetColumn(2) ;
                Shader.SetGlobalVector(Shader.PropertyToID("_LightColor0"), visibleLight.finalColor);
                Shader.SetGlobalVector(Shader.PropertyToID("_WorldSpaceLightPos0"), new Vector4(dir.x,dir.y,dir.z,0.0f) );
                break;
            }
        }
    }
}

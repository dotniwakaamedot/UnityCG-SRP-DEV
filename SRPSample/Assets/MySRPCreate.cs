using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

#if UNITY_EDITOR

// エディタ拡張
public class MySRPCreate
{
    [MenuItem("AssetsItem/Create/MySRP")]
    public static void CreateSRP()
    {
        var instance = ScriptableObject.CreateInstance<MySRPAsset>();
        AssetDatabase.CreateAsset(instance, "Assets/MyScriptableRenderPipeline.asset");
//        GraphicsSettings.renderPipelineAsset = instance;
    }
}

#endif
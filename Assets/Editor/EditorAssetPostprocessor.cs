using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

public class EditorAssetPostprocessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        var texture = assetImporter as TextureImporter;
        if (assetPath.Contains("Lightmap-") == true)
        {
            texture.mipmapEnabled = false;
            TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
            platformSettings.overridden = true;
            platformSettings.maxTextureSize = 256;
            platformSettings.format = TextureImporterFormat.ETC2_RGBA8;
            texture.SetPlatformTextureSettings(platformSettings);
        }
    }

    void OnPreprocessModel()
    {
        var model = assetImporter as ModelImporter;
        if (assetPath.Contains("人物") && assetPath.Contains("Res/Character"))
        {

        }
    }

    void OnPostprocessModel(GameObject g)
    {
        AnimationClip[] clipArray = AnimationUtility.GetAnimationClips(g);
        List<AnimationClip> animationClipList = new List<AnimationClip>(clipArray);

        if (animationClipList.Count == 0)
        {
            AnimationClip[] objectList = UnityEngine.Object.FindObjectsOfType(typeof(AnimationClip)) as AnimationClip[];
            animationClipList.AddRange(objectList);
        }
        foreach (AnimationClip theAnimation in animationClipList)
        {

            try
            {
                //去除scale曲线
                foreach (EditorCurveBinding theCurveBinding in AnimationUtility.GetCurveBindings(theAnimation))
                {
                    string name = theCurveBinding.propertyName.ToLower();
                    if (name.Contains("scale"))
                    {
                        AnimationUtility.SetEditorCurve(theAnimation, theCurveBinding, null);
                    }
                }

                //浮点数精度压缩到f3
                AnimationClipCurveData[] curves = null;
                curves = AnimationUtility.GetAllCurves(theAnimation);
                Keyframe key;
                Keyframe[] keyFrames;
                for (int ii = 0; ii < curves.Length; ++ii)
                {
                    AnimationClipCurveData curveDate = curves[ii];
                    if (curveDate.curve == null || curveDate.curve.keys == null)
                    {
                        continue;
                    }
                    keyFrames = curveDate.curve.keys;
                    for (int i = 0; i < keyFrames.Length; i++)
                    {
                        key = keyFrames[i];
                        key.value = float.Parse(key.value.ToString("f3"));
                        key.inTangent = float.Parse(key.inTangent.ToString("f3"));
                        key.outTangent = float.Parse(key.outTangent.ToString("f3"));
                        keyFrames[i] = key;
                    }
                    curveDate.curve.keys = keyFrames;
                    theAnimation.SetCurve(curveDate.path, curveDate.type, curveDate.propertyName, curveDate.curve);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("CompressAnimationClip Failed !!! animationPath : {0} error: {1}", assetPath, e));
            }

            if (theAnimation.name == "Take 001")
            {
                theAnimation.name = g.name;
            }
        }
    }

    public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {

    }
}

using UnityEngine;
using System.Collections;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEditor;

namespace EDT
{
public class EditorResourceBuild
{
    static public Dictionary<string, string> m_mapAssetList = new Dictionary<string, string>();

    public static string ASSET_PATH_KEY = "fzgdhks";

    static string GetPlatformName(BuildTarget buildTarget)
    {
        string szFolerName = "Windows";
        switch (buildTarget)
        {
            case BuildTarget.Android:
                szFolerName = "Android";
                break;

            case BuildTarget.iOS:
                szFolerName = "iOS";
                break;
        }
        return szFolerName;
    }

    static string GetAssetBundleOutPutPath(BuildTarget buildTarget)
    {
        return Application.dataPath.Replace("/Assets", string.Empty) + "/AssetBundles" + "/" + GetPlatformName(buildTarget) + "/";
    }
    static bool DeleteBundles()
    {
        if (Directory.Exists(GetAssetBundleOutPutPath(EditorUserBuildSettings.activeBuildTarget)))
        {
            Directory.Delete(GetAssetBundleOutPutPath(EditorUserBuildSettings.activeBuildTarget), true);
        }

        Directory.CreateDirectory(GetAssetBundleOutPutPath(EditorUserBuildSettings.activeBuildTarget));

        return true;
    }

    static bool DeleteBundleNames()
    {
        string[] oldAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        if(oldAssetBundleNames.Length <= 0)
        {
            return true;
        }

        for (var i = 0; i < oldAssetBundleNames.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(oldAssetBundleNames[i], true);
        }

        return true;
    }

    static bool SetBundleConfig()
    {
        UnityEngine.Object[] assets = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        for (int i = 0; i < assets.Length; i++)
        {
            UnityEngine.Object obj = assets[i];
            string assetPath = AssetDatabase.GetAssetPath(obj);
            string extenName = System.IO.Path.GetExtension(assetPath).ToLower();
            if (string.IsNullOrEmpty(extenName) || extenName == ".meta")
            {
                continue;
            }
            switch (extenName)
            {
                case ".xml":
                case ".txt":
                {
                    EditorResourceBuild.m_mapAssetList[assetPath] = obj.name + extenName + ".assetbundle";
                }
                break;
                case ".prefab":
                {
                    EditorResourceBuild.m_mapAssetList[assetPath] = GTTools.GetParentPathName(assetPath) + ".pre.assetbundle";
                }
                break;
                case ".mp3":
                {
                    EditorResourceBuild.m_mapAssetList[assetPath] = obj.name + extenName + ".assetbundle";
                }
                break;
                case ".png":
                {
                    if (assetPath.Contains("Textures"))
                    {
                        EditorResourceBuild.m_mapAssetList[assetPath] = GTTools.GetParentPathName(assetPath) + ".atlas.assetbundle";
                    }
                }
                break;
            }
        }

        XmlDocument doc = new XmlDocument();
        XmlNode root = doc.CreateElement("root");
        doc.AppendChild(root);
        foreach (var current in EditorResourceBuild.m_mapAssetList)
        {
            XmlElement child = doc.CreateElement("row");
            root.AppendChild(child);
            child.SetAttribute("AssetName", current.Key);
            child.SetAttribute("AssetBundleName", current.Value.ToLower());
        }

        string fileName = GetAssetBundleOutPutPath(EditorUserBuildSettings.activeBuildTarget) + "/assetlist.xml";
        doc.Save(fileName);
        return true;
    }

    static bool SetBundleNames()
    {
        foreach (var current in EditorResourceBuild.m_mapAssetList)
        {
            var assetImporter = AssetImporter.GetAtPath(current.Key);
            if (assetImporter != null)
            {
                assetImporter.assetBundleName = current.Value.ToLower();
            }
        }

        return true;
    }
    public static string GetMD5(byte[] src)
    {
        var md5 = System.Security.Cryptography.MD5.Create();
        byte[] bytes = md5.ComputeHash(src);
        md5.Clear();

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            sb.Append(bytes[i].ToString("x2"));
        }

        return sb.ToString();
    }
    public static string GetAssetPathMd5(string path)
    {
        string tmp = path + EditorResourceBuild.ASSET_PATH_KEY;
        byte[] data = System.Text.Encoding.UTF8.GetBytes(tmp);
        return GetMD5(data);
    }

    static void BuildBundles()
    {
        BuildPipeline.BuildAssetBundles(GetAssetBundleOutPutPath(EditorUserBuildSettings.activeBuildTarget), BuildAssetBundleOptions.UncompressedAssetBundle, EditorUserBuildSettings.activeBuildTarget);
    }

    public static void Build()
    {
        if(!DeleteBundles())
        {
            return;
        }


        //if (!DeleteBundleNames())
        //{
        //    return;
        //}

        //return;
        if (!SetBundleConfig())
        {
            return;
        }
        SetBundleNames();
        BuildBundles();
    }
}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class GTResourceManager : GTSingleton<GTResourceManager>
{
    public string m_EditorResourceRoot = "Assets/Resources/";

    protected void InitializeResource(Action initOK)
    {
    }
    string GetResourcePath(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            return assetPath;
        }

        if (assetPath.StartsWith(m_EditorResourceRoot))
        {
            assetPath = assetPath.Substring(m_EditorResourceRoot.Length);
        }

        string sPath = assetPath;

        int nDotPos = sPath.LastIndexOf('.');
        if (nDotPos > 0)
        {
            sPath = sPath.Substring(0, nDotPos);
        }

        return sPath;
    }

protected T LoadEditorAsset<T>(string assetPath, bool instance = false) where T :
    UnityEngine.Object
    {
        string sShortPath = GetResourcePath(assetPath);
        sShortPath.Replace("\\", "/");
        sShortPath.TrimEnd("\\/".ToCharArray());

        T tAsset = Resources.Load<T>(sShortPath) as T;
        if (tAsset == null)
        {
            Debug.LogError(string.Format("GTResourceManager Load Failed ! assetPath : {0}", assetPath));
            return null;
        }

        if (instance)
        {
            return UnityEngine.Object.Instantiate(tAsset);
        }

        return tAsset;
    }

    protected void UnloadEditorAsset(string abName)
    {

    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;
using UnityEngine.Networking;

public partial class GTResourceManager : GTSingleton<GTResourceManager>
{
    public ResourceType m_ResourceType = ResourceType.TYPE_SOURCE;                 //资源加载方式
    void Awake()
    {
        m_ResourceType = ResourceType.TYPE_SOURCE;
    }

    override public void Init()
    {
        if(m_ResourceType == ResourceType.TYPE_SOURCE)
        {
            InitializeResource(null);
        }
        else
        {
            InitializeBundle(null);
        }
    }

    public string GetExtPath()
    {
        string path = string.Empty;
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                path = Application.streamingAssetsPath;
                break;
            case RuntimePlatform.Android:
                path = Application.persistentDataPath;
                break;
            case RuntimePlatform.IPhonePlayer:
                path = Application.persistentDataPath;
                break;
            default:
                path = Application.streamingAssetsPath;
                break;
        }
        return path;
    }

    public string GetStreamingPath()
    {
        string path = string.Empty;
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                path = Application.streamingAssetsPath + "/Windows";
                break;
            case RuntimePlatform.Android:
                path = Application.streamingAssetsPath + "/Android";
                break;
            case RuntimePlatform.IPhonePlayer:
                path = Application.streamingAssetsPath + "/iOS";
                break;
            default:
                path = Application.streamingAssetsPath + "/Windows";
                break;
        }
        return path;
    }

public T Load<T>(string assetPath, bool instance = false) where T :
    UnityEngine.Object
    {
        if (m_ResourceType == ResourceType.TYPE_SOURCE)
        {
            return LoadEditorAsset<T>(assetPath, instance);
        }
        else
        {
            return LoadBundleAsset<T>(assetPath, instance);
        }

    }

    public void DestroyObj(UnityEngine.Object obj)
    {
        if (obj != null)
        {
            UnityEngine.Object.DestroyObject(obj);
        }
    }

    void CopyPathToPath(string sFromPath, string sToPath)
    {
        Debug.LogError("**********Start to CopyPathToPath sFromPath:" + sFromPath + ", sToPath:" + sToPath);

        DirectoryInfo dirInfo = new DirectoryInfo(sFromPath);

        FileInfo[] files = dirInfo.GetFiles();
        for (int j = 0; j < files.Length; j++)
        {
            if (files[j].FullName.Contains("meta"))
            {
                continue;
            }
            string targetFile = Path.Combine(sToPath, files[j].Name);
            string fromFile = files[j].FullName;
            UnityWebRequest uwr = UnityWebRequest.Get(fromFile);
            uwr.SendWebRequest();
            while(!uwr.isDone)
            {
                Debug.LogError("**********Wait To ReadFile Path:" + fromFile);
            }

            File.WriteAllBytes(targetFile, uwr.downloadHandler.data);
        }
        DirectoryInfo[] directories = dirInfo.GetDirectories();

        for (int i = 0; i < directories.Length; i++)
        {
            CopyPathToPath(directories[i].FullName, Path.Combine(sToPath, directories[i].Name));
        }
    }

    public void CheckAndCopyToDataPath()
    {
        Debug.Log("+++++++++Start to Copy File To Data Path+++++++++++++++++++++++");

        return;

        string szToVersoinPath = Path.Combine(GetBundlePath(), "version.txt");
        if(File.Exists(szToVersoinPath))
        {
            Debug.Log("----------Start to Copy File To Data Path-----path:" + szToVersoinPath);
            return;
        }

        //开始拷贝  先加载原文件 按照原stremingassets路径
        string szFromVersoinPath = Path.Combine(GetStreamingPath(), "version.txt");
        UnityWebRequest uwr = UnityWebRequest.Get(szFromVersoinPath);
        uwr.SendWebRequest();
        while (!uwr.isDone)
        {
            Debug.Log("----------Wait To Read version.txt-----path:" + szFromVersoinPath);
        }

        File.WriteAllBytes(szToVersoinPath, uwr.downloadHandler.data);

        if (!Directory.Exists(GetBundlePath()))
        {
            Directory.CreateDirectory(GetBundlePath());
        }

        CopyPathToPath(GetStreamingPath(), GetBundlePath());

        Debug.Log("+++++++++End to Copy File To Data Path+++++++++++++++++++++++");

        return;
    }


}

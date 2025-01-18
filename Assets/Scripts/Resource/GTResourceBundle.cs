using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using System.IO;
using UnityEngine.Networking;

class GTAssetRequest
{
    public Type assetType;
    public string assetNames;
    public List<Action<UnityEngine.Object>> callBackList;
    public bool isDepAsset;
}

class GTBundleRequest
{
    public string bundleNames;
    public List<GTAssetRequest> assetReqList;
}

public partial class GTResourceManager : GTSingleton<GTResourceManager>
{
    public string m_strResourceRoot = "Assets/Resources/";

    public AssetBundleManifest m_RootManifest;

    Dictionary<string, GTAssetRequest> m_AssetRequests = new Dictionary<string, GTAssetRequest>();

    Dictionary<string, GTBundleRequest> m_BundleRequests = new Dictionary<string, GTBundleRequest>();

    Dictionary<string, AssetBundle> m_LoadedAssetBundles = new Dictionary<string, AssetBundle>();

    public Dictionary<string, string> m_mapAsset2Bundle = new Dictionary<string, string>();
    protected void InitializeBundle(Action initOK)
    {
        LoadAssetList();

        if (!LoadRootManifest())
        {
            Debug.LogError(string.Format("InitializeBundle Failed  Load Root Manifest failed!"));
            return;
        }

        if (initOK != null)
        {
            initOK();
        }
    }

    public bool LoadRootManifest()
    {
        string strMainPath = Path.Combine(GetBundlePath(), GetPlatformName());
        AssetBundle tAbData = null;
        if (File.Exists(strMainPath))
        {
            tAbData = AssetBundle.LoadFromFile(strMainPath);
        }
        else
        {
            tAbData = AssetBundle.LoadFromFile(Path.Combine(GetStreamingPath(), GetPlatformName()));

        }

        if (tAbData == null)
        {
            Debug.LogError(string.Format("LoadRootManifest Failed can not load assetbundle!"));
            return false;
        }

        m_RootManifest = (AssetBundleManifest)tAbData.LoadAsset("AssetBundleManifest");
        if(m_RootManifest == null)
        {
            Debug.LogError(string.Format("LoadRootManifest Failed can not load manifest asset!"));
            return false;
        }

        return true;
    }

    public AssetBundle LoadBundle(string strAbName)
    {
        AssetBundle tAbData = null;
        if (m_LoadedAssetBundles.TryGetValue(strAbName, out tAbData))
        {
            return tAbData;
        }

        string[] allDependencies = m_RootManifest.GetAllDependencies(strAbName);
        for(int i = 0; i < allDependencies.Length; i++)
        {
            LoadBundle(allDependencies[i]);
        }

        string sAbPath = Path.Combine(GetBundlePath(), strAbName);
        if (File.Exists(sAbPath))
        {
            tAbData = AssetBundle.LoadFromFile(sAbPath);
        }
        else
        {
            tAbData = AssetBundle.LoadFromFile(Path.Combine(GetStreamingPath(), strAbName));
        }

        if (tAbData == null)
        {
            Debug.LogError(string.Format("LoadBundleAsset Failed can not load strAbName:{0}!", strAbName));
            return null;
        }

        m_LoadedAssetBundles[strAbName] = tAbData;

        return tAbData;
    }

public T LoadBundleAsset<T>(string assetPath, bool instance = false) where T :
    UnityEngine.Object
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            return null;
        }

        if (!assetPath.StartsWith(m_strResourceRoot))
        {
            assetPath = Path.Combine(m_strResourceRoot, assetPath);
        }

        string strAbName = GetAbNameByPath(assetPath);
        if(string.IsNullOrEmpty(strAbName))
        {
            Debug.LogError(string.Format("LoadBundleAsset Failed can not GetAbNameByPath assetPath:{0} !", assetPath));
            return null;
        }

        AssetBundle tAbData = LoadBundle(strAbName);
        if(tAbData == null)
        {
            Debug.LogError(string.Format("LoadBundleAsset Failed can not load [BUNDLE]; assetPath:{0} !", assetPath));
            return null;
        }

        UnityEngine.Object tObject = tAbData.LoadAsset(assetPath);
        if(tObject == null)
        {
            Debug.LogError(string.Format("LoadBundleAsset Failed can not load [ASSET]; assetPath:{0} !", assetPath));
            return null;
        }

        if(instance)
        {
            return GameObject.Instantiate(tObject) as T;
        }

        return tObject as T;
    }

public bool LoadBundleAssetAsync<T>(string assetPath, Action<UnityEngine.Object> action = null) where T :
    UnityEngine.Object
    {
        string strAbName = GetAbNameByPath(assetPath);
        string sAbPath = Path.Combine(GetBundlePath(), strAbName);
        sAbPath = sAbPath.Replace("\\", "/");

        GTAssetRequest tAssetReq;
        if(!m_AssetRequests.TryGetValue(assetPath, out tAssetReq))
        {
            tAssetReq = new GTAssetRequest();
            tAssetReq.assetType = typeof(T);
            tAssetReq.assetNames = assetPath;
            tAssetReq.callBackList = new List<Action<UnityEngine.Object>>();
            tAssetReq.callBackList.Add(action);
            tAssetReq.isDepAsset = false;
        }
        else
        {
            tAssetReq.callBackList.Add(action);
        }

        GTBundleRequest tBundleReq;
        if (!m_BundleRequests.TryGetValue(strAbName, out tBundleReq))
        {
            tBundleReq = new GTBundleRequest();
            tBundleReq.bundleNames = strAbName;
            tBundleReq.assetReqList = new List<GTAssetRequest>();
            tBundleReq.assetReqList.Add(tAssetReq);
        }
        else
        {
            if(tBundleReq.assetReqList == null)
            {
                tBundleReq.assetReqList = new List<GTAssetRequest>();
            }

            tBundleReq.assetReqList.Add(tAssetReq);
            m_BundleRequests[strAbName] = tBundleReq;
        }

        GTCoroutinueManager.Instance.StartCoroutine(LoadBundleHandler<T>(strAbName)); //开始加载bundle

        return true;
    }


    IEnumerator WaitDepAssetBundleLoaded(string abName)
    {
        AssetBundle bundle = null;
        while (!m_LoadedAssetBundles.TryGetValue(abName, out bundle))
        {
            yield return new WaitForEndOfFrame();
        }

        yield return 0;
    }

IEnumerator LoadBundleHandler<T>(string abName) where T :
    UnityEngine.Object
    {
        AssetBundle tBundle = GetLoadedAssetBundle(abName);
        if (tBundle != null)
        {
            yield return GTCoroutinueManager.Instance.StartCoroutine(OnLoadBundleSuccess<T>(abName)); //开始加载bundle里的资源
            yield break;
        }

        string[] dependencies = m_RootManifest.GetAllDependencies(abName);
        for (int i = 0; i < dependencies.Length; i++)
        {
            string depName = dependencies[i];
            if (GetLoadedAssetBundle(depName) != null)
            {
                continue;
            }

            if (!m_BundleRequests.ContainsKey(depName))
            {
                GTBundleRequest tBundleReq = new GTBundleRequest();
                tBundleReq.bundleNames = depName;
                m_BundleRequests[depName] = tBundleReq;
                yield return LoadBundleHandler<T>(depName); //开始加载bundle
            }
            else
            {
                yield return WaitDepAssetBundleLoaded(depName);
            }
        }

        string sAssetBundlePath = Path.Combine(GetBundlePath(), abName);
        sAssetBundlePath.Replace("\\", "/");
        AssetBundleCreateRequest bundleReq = AssetBundle.LoadFromFileAsync(sAssetBundlePath);
        yield return bundleReq;
        if (!bundleReq.isDone)
        {
            yield break;
        }

        m_LoadedAssetBundles[abName] = bundleReq.assetBundle;

        yield return GTCoroutinueManager.Instance.StartCoroutine(OnLoadBundleSuccess<T>(abName)); //开始加载bundle里的资源
    }

    public string GetBundlePath()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                return Application.persistentDataPath + "/Android";
            case RuntimePlatform.IPhonePlayer:
                return Application.persistentDataPath + "/iOS";
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                return Application.streamingAssetsPath + "/Windows";
            default:
                return Application.streamingAssetsPath + "/Windows";
        }
    }

    static string GetPlatformName()
    {
        string szFolerName = "Windows";
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                szFolerName = "Android";
                break;
            case RuntimePlatform.IPhonePlayer:
                szFolerName = "iOS";
                break;
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                szFolerName = "Windows";
                break;
        }
        return szFolerName;
    }

    public void LoadAssetList()
    {
        XmlDocument doc = new XmlDocument();
        string sPath = Path.Combine(GetBundlePath(), "assetlist.xml");
        if (File.Exists(sPath))
        {
            doc.Load(sPath);
        }
        else
        {
            string szStreamingAssetPath = Path.Combine(GetStreamingPath(), "assetlist.xml");
            UnityWebRequest uwr = UnityWebRequest.Get(szStreamingAssetPath);
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                Debug.Log("----------Wait To Load AssetList From Steaming -----sAssetPath:" + "assetlist.xml");
            }

            doc.LoadXml(uwr.downloadHandler.text);
        }

        XmlElement root = doc.FirstChild as XmlElement;
        XmlElement child = root.FirstChild as XmlElement;
        while (child != null)
        {
            m_mapAsset2Bundle[child.GetString("AssetName")] = child.GetString("AssetBundleName");
            child = child.NextSibling as XmlElement;
        }
    }

    public string GetAbNameByPath(string assetPath)
    {
        string strOutPath = "";
        m_mapAsset2Bundle.TryGetValue(assetPath, out strOutPath);
        return strOutPath;
    }

    public AssetBundle GetLoadedAssetBundle(string abName)
    {
        AssetBundle bundle = null;
        m_LoadedAssetBundles.TryGetValue(abName, out bundle);
        return bundle;
    }

//响应ab加载成功
IEnumerator OnLoadBundleSuccess<T>(string abName) where T :
    UnityEngine.Object
    {
        AssetBundle bundleData = GetLoadedAssetBundle(abName);
        if (bundleData == null)
        {
            yield break;
        }

        GTBundleRequest tBundleReq = null;
        if (!m_BundleRequests.TryGetValue(abName, out tBundleReq))
        {
            m_BundleRequests.Remove(abName);
            yield break;
        }

        m_AssetRequests.Remove(abName);
        for (int i = 0; i < tBundleReq.assetReqList.Count; i++)
        {
            string assetNames = tBundleReq.assetReqList[i].assetNames;

            AssetBundleRequest assetReq = bundleData.LoadAssetAsync<T>(assetNames);
            yield return assetReq;

            if (assetReq.asset == null)
            {
                continue;
            }

            for(int j  = 0; j < tBundleReq.assetReqList[i].callBackList.Count; j++)
            {
                tBundleReq.assetReqList[i].callBackList[j](assetReq.asset);
            }
        }
    }

    public void UnloadAssetBundle(string abName, bool all = false)
    {
        AssetBundle ab = null;
        m_LoadedAssetBundles.TryGetValue(abName, out ab);
        if (ab != null)
        {
            ab.Unload(all);
        }
    }
    public void UnloadAssetBundleByAssetPath(string assetPath, bool all = false)
    {
        string strAbName = GetAbNameByPath(assetPath);

        UnloadAssetBundle(strAbName);
    }

    public void LoadFromStreamingAssets(string sPath, string pPath)
    {
        string targetPath = Application.persistentDataPath + sPath;
        string sourcePath = Application.streamingAssetsPath + pPath;
        if (Application.platform == RuntimePlatform.Android)
        {
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            WWW www = new WWW(sourcePath);
            bool boo = true;
            while (boo)
            {
                if (www.isDone)
                {
                    File.WriteAllBytes(targetPath, www.bytes);
                    boo = false;
                }
            }
        }
    }

}
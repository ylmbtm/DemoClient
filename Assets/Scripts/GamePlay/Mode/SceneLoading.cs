using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Protocol;
using UnityEngine.SceneManagement;

public sealed class SceneLoading : IScene
{ 
    private float              mWaitTime = 0.1f;
    private float              mWaitTimer = 0f;
    private bool               mLoading = false;
    private int                mLoadingCopyID;
    private UILoading          mLoadingWindow;
    private AsyncOperation     mAsync;
    private Callback           mCallback;

    public          void SetSceneID(int sceneID, Callback onFinish)
    {
        this.mLoadingCopyID = sceneID;
        this.mCallback      = onFinish;
    }

    public override void Enter()
    {
        base.Enter();
        GTWorld.         Instance.PauseGuide(true);
        GTWindowManager. Instance.Release();
        GTWorld.         Instance.LeaveWorld();
        GTPoolManager.   Instance.Release();
        GTWindowManager. Instance.OpenWindow(EWindowID.UILoading);
        mLoadingWindow  = (UILoading)GTWindowManager.Instance.GetWindow(EWindowID.UILoading);
        mWaitTime       = Time.realtimeSinceStartup;
    }

    public override void Execute()
    {
        if (mLoading == false)
        {
            if (Time.realtimeSinceStartup - mWaitTime > 0.1f)
            {
                mAsync = LoadSceneByID(mLoadingCopyID);
                mLoading = true;
            }
        }
        if (mAsync != null)
        {
            mAsync.allowSceneActivation = false;
            if (mLoadingWindow.transform != null)
            {
                mLoadingWindow.UpdateProgress(mAsync.progress);
            }
            while (mAsync.progress < 0.9f)
            {
                return;
            }
            mAsync.allowSceneActivation = true;
            if (mAsync.isDone)
            {
                mLoading = false;
                mAsync = null;
                OnSceneWasLoaded();
            }
        }    
    }

    public override void Exit()
    {
        mWaitTimer = 0;
        mLoading = false;
        mAsync = null;
        GTTimerManager.Instance.AddTimer(0.2f, mCallback);
        GTTimerManager.Instance.AddTimer(1.0f, OnSceneWasFadeOut);
    }

    void OnSceneWasLoaded()
    {
        if (GTCameraManager.Instance.MainCamera != null)
        {
            for (int i = 0; i < Camera.allCameras.Length; i++)
            {
                Camera cam = Camera.allCameras[i];
                AudioListener listener = cam.GetComponent<AudioListener>();
                if (cam != GTCameraManager.Instance.MainCamera && listener!=null)
                {
                    listener.enabled = false;
                    UnityEngine.Object.DestroyImmediate(listener);
                }
            }
        }
        DCopy db = ReadCfgCopy.GetDataById(mLoadingCopyID);
        GTAudioManager.Instance.PlayMusic(db.SceneMusic);
        GTLauncher.    Instance.LoadState(GTLauncher.Instance.NextCopyType);
        switch (db.CopyType)
        {
            //只有登录和选角是从这里进入场景，其它的都是网络协义进场景
            case ECopyType.TYPE_LOGIN:
            case ECopyType.TYPE_ROLE:
                GTWorld.Instance.EnterWorld(mLoadingCopyID);
                break;
        }
    }

    void OnSceneWasFadeOut()
    {
        Resources.UnloadUnusedAssets();
        GC.Collect();
        GTData.CopyID = mLoadingCopyID;
        GTWindowManager.Instance.HideWindow(EWindowID.UILoading);
        GTWorld.Instance.PauseGuide(false);
        mCallback = null;
    }

    AsyncOperation LoadSceneByID(int id)
    {
        DCopy db = ReadCfgCopy.GetDataById(id);
        if (string.IsNullOrEmpty(db.SceneName))
        {
            return null;
        }
        Resources.UnloadUnusedAssets();
        GC.Collect();
        AsyncOperation async = SceneManager.LoadSceneAsync(db.SceneName);
        return async;
    }
}

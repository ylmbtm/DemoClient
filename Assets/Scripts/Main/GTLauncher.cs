using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Xml;
using System.IO;
using System.Reflection;

public class GTLauncher : MonoBehaviour
{
    public static GTLauncher  Instance;

    public string        CurrSceneName;
    public ECopyType     CurrCopyType;
    public ECopyType     NextCopyType;
    public bool          UseFPS;
    public bool          MusicDisable = true;
    public bool          UseGuide = true;
    public IScene        CurScene;
    public Int32         LastCityID;
    public string        LoginIP = "127.0.0.1";
    public int           LoginPort = 9001;

    private IStateMachine<GTLauncher, ECopyType> mStateMachine;

    void Awake()
    {
        Time.timeScale = 1;
        Application.runInBackground = true;
        GTData.IsLaunched = true;
        GTData.LastCityID = LastCityID;
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ShowFPS();
        PlayCGMovie();
    }

    void Start()
    {
        InitBehavic();
        IgnorePhysicsLayer();
        InitFSM();
        InitManager();
        InitTag();
        LoadStartScene();
    }

    void InitTag()
    {
        GTLog.Open(GTLogTag.TAG_ACTOR);
    }

    public void LoadStartScene()
    {
        GTLauncher.Instance.LoadScene(GTCopyKey.SCENE_LOGIN);
    }
    private static string BehaviacFilePath
    {
        get
        {
            string relativePath = "/Resources/behaviac/exported";

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                return Application.dataPath + relativePath;
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                return Application.dataPath + relativePath;
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                return Application.dataPath + relativePath;
            }
            else
            {
                return "Assets" + relativePath;
            }
        }
    }
    void InitManager()
    {
        NetworkManager.       Instance.Init();
        GTResourceManager.    Instance.Init();
        GTData.               Instance.Init();
        GTCoroutinueManager.  Instance.SetRoot(transform);
        GTAudioManager.       Instance.SetRoot(transform);
        GTCameraManager.      Instance.SetRoot(transform);
        GTInputManager.       Instance.SetRoot(transform);
        GTInput.              Instance.SetRoot(transform);
        GTAudioManager.       Instance.SetMusicActive(!MusicDisable);
        GTConfigManager.      Instance.Init();
        GTWindowManager.      Instance.Init();
        GTWorld.              Instance.SetRoot(transform);
        GTWorld.              Instance.EnterWorld(GTCopyKey.SCENE_LAUNCHER);
        GTTimerManager.       Instance.AddTimer(1, SecondTick, 0);
        GTNetworkRecv.        Instance.AddListener();
    }

    void InitFSM()
    {
        this.mStateMachine = new IStateMachine<GTLauncher, ECopyType>(this, new GTCopyTypeTypeComparer());
        this.mStateMachine.AddState(ECopyType.TYPE_INIT,  new SceneInit());
        this.mStateMachine.AddState(ECopyType.TYPE_LOGIN, new SceneLogin());
        this.mStateMachine.AddState(ECopyType.TYPE_LOAD,  new SceneLoading());
        this.mStateMachine.AddState(ECopyType.TYPE_ROLE,  new SceneRole());
        this.mStateMachine.AddState(ECopyType.TYPE_CITY,  new SceneCity());
        this.mStateMachine.AddState(ECopyType.TYPE_PVE,   new ScenePVE());
        this.mStateMachine.AddState(ECopyType.TYPE_WORLD, new SceneWorld());
        this.mStateMachine.SetCurState(this.mStateMachine.GetState(ECopyType.TYPE_INIT));
        this.CurScene = (IScene)this.mStateMachine.GetState(ECopyType.TYPE_INIT);
    }

    void SecondTick()
    {
        GTEventCenter.FireEvent(GTEventID.TYPE_TICK_SECOND);
    }

    void IgnorePhysicsLayer()
    {
        Physics.IgnoreLayerCollision(GTLayer.LAYER_PLAYER,  GTLayer.LAYER_PET);
        Physics.IgnoreLayerCollision(GTLayer.LAYER_PLAYER,  GTLayer.LAYER_PLAYER);
        //Physics.IgnoreLayerCollision(GTLayer.LAYER_PLAYER,  GTLayer.LAYER_ACTOR);
        //Physics.IgnoreLayerCollision(GTLayer.LAYER_PET,     GTLayer.LAYER_ACTOR);
        //Physics.IgnoreLayerCollision(GTLayer.LAYER_PET,     GTLayer.LAYER_PET);
        //Physics.IgnoreLayerCollision(GTLayer.LAYER_MONSTER, GTLayer.LAYER_MONSTER);
        Physics.IgnoreLayerCollision(GTLayer.LAYER_ACTOR,   GTLayer.LAYER_ACTOR);
        Physics.IgnoreLayerCollision(GTLayer.LAYER_PET,     GTLayer.LAYER_BARRER);
    }



    public void LoadState(ECopyType state)
    {
        if (CurrCopyType == state)
        {
            return;
        }
        this.mStateMachine.ChangeState(state);
        this.CurrCopyType = state;
        this.CurrSceneName = SceneManager.GetActiveScene().name;
        this.CurScene = (IScene)this.mStateMachine.GetState(state);
    }

    public void LoadScene(int sceneId, Callback finishCallback = null)
    {
        DCopy db = ReadCfgCopy.GetDataById(sceneId);
        this.NextCopyType = db.CopyType;
        SceneLoading sceneLoading = (SceneLoading)mStateMachine.GetState(ECopyType.TYPE_LOAD);
        sceneLoading.SetSceneID(sceneId, finishCallback);
        LoadState(ECopyType.TYPE_LOAD);
    }

    public void ShowFPS()
    {
        if (UseFPS)
        {
            GameObject go = new GameObject("FPS");
            go.AddComponent<EFPS>();
            go.transform.parent = transform;
        }
    }

    public void PlayCGMovie()
    {
        //Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //Handheld.PlayFullScreenMovie("CG.mp4", Color.black, FullScreenMovieControlMode.CancelOnInput, FullScreenMovieScalingMode.AspectFit);
    }

    void Update()
    {
        behaviac.Workspace.Instance.DebugUpdate();

        if (this.mStateMachine != null)
        {
            this.mStateMachine.Execute();
        }
        GTTimerManager.  Instance.Execute();
        GTUpdate.        Instance.Execute();
        NetworkManager.  Instance.Execute();
        GTWorld.         Instance.Execute();
        GTAsync.         Instance.Execute();
    }

    void FixedUpdate()
    {
        GTAction.Update();
    }

    void LateUpdate()
    {
        GTUpdate.Instance.ExecuteLateUpdate();
    }

    void OnApplicationQuit()
    {
        CleanupBehaviac();
        GTTimerManager.Instance.DelTimer(SecondTick);
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    public bool InitBehavic()
    {
        // behaviac.Config.IsSocketBlocking = true;
        //behaviac.Config.IsLogging = true;
        // behaviac.Config.SocketPort = 60636;
        behaviac.Workspace.Instance.FilePath = BehaviacFilePath;
        behaviac.Workspace.Instance.FileFormat = behaviac.Workspace.EFileFormat.EFF_xml;
        return true;
    }


    public void CleanupBehaviac()
    {
        behaviac.Workspace.Instance.Cleanup();
    }
}

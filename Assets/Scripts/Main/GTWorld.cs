using ACT;
using BIE;
using CUT;
using FLY;
using FWD;
using HUD;
using MAP;
using PLT;
using TAS;
using Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECT;
using MIE;

public class GTWorld : GTMonoSingleton<GTWorld>
{
    private GameObject            m_TargetCircle;
    public EffectSystem           Effect         { get; private set; }
    public FlywordSystem          Flyword        { get; private set; }
    public GuideSystem            Guide          { get; private set; }
    public PlotSystem             Plot           { get; private set; }
    public VideoSystem            Video          { get; private set; }
	public HUDBlood            	  HudBlood       { get;  set; }
    public MAPWorldMap            WorldMap       { get; private set; }
    public MAPWorldLandscape      WorldLandscape { get; private set; }
    public int                    WorldMapID     { get; private set; }

	public static List<Actor>     Characters = new List<Actor>();

	public static List<FlyObject> FlyObjects = new List<FlyObject>();

	public static List<Mineral>   Minerals   = new List<Mineral>();
	public static Actor           Main { get; set; }
	public static Actor           Boss { get; set; }
    public GameObject             TargetCircle
    {
        get
        {
            if (m_TargetCircle == null)
            {
                m_TargetCircle = GTResourceManager.Instance.Load<GameObject>("Effect/other/targetcircle", true);
            }
            return m_TargetCircle;
        }
    }

	public override void SetRoot(Transform parent)
	{
		base.SetRoot(parent);
		this.Effect  = new EffectSystem();
		this.Flyword = new FlywordSystem();
		this.Guide   = new GuideSystem();
		this.Plot    = new PlotSystem();
		this.Video   = new VideoSystem();

		GameObject g = new GameObject(string.Format("HudBloodRoot"));
		HudBlood = g.AddComponent<HUDBlood>();
		g.transform.parent = parent;
		g.SetActive (true);
		return;
	}

    public Actor AddActor(XCharacter data, bool isNativePlayer = false)
    {
        GameObject g = new GameObject(string.Format("Actor {0} {1}", data.Id.ToString("000000"), data.GUID));
        Actor a = g.AddComponent<Actor>();
        g.transform.parent = transform;
        a.IsNativeActor = isNativePlayer;
       
        a.Initial(data);
        Characters.Add(a);
        GTEventCenter.FireEvent(GTEventID.TYPE_MAP_CREATE_ACTOR, a.GUID);
        return a;
    }

    public Actor AddActorForDisplay(int id)
    {
        XCharacter data = new XCharacter();
        data.Id = id;
        data.GUID = GTData.NewGUID;
        data.Camp = 0;
        Actor actor = AddActor(data, false);
        actor.EnableCharacter(false);
        actor.Add<ActorAvator>();
        actor.Add<ActorAnimation>();
        actor.Get<ActorAnimation>().Play("idle");
        actor.Startup(data);
        return actor;
    }

    public Actor GetActor(ulong guid)
    {
        for (int i = 0; i < Characters.Count; i++)
        {
            if (Characters[i].GUID == guid)
            {
                return Characters[i];
            }
        }
        return null;
    }

    public void  DelActor(Actor actor)
    {
        if (actor == null)
        {
            Debug.LogError("Error DelActor actor is null!");
            return;
        }

		ActorEffect aEffect = actor.Get<ActorEffect>();
        if (aEffect != null)
        {
            aEffect.Release();
        }

		HudBlood.RemoveHudBlood (actor);
			
        Characters.Remove(actor);
        GTPoolManager.Instance.ReleaseGo(actor.DBModel.Model, actor.Obj);
          
	    GameObject.Destroy(actor.CacheTransform.gameObject);
		UnityEngine.Object.Destroy(actor);
    }

    public void  DelActor(ulong guid)
    {
        Actor actor = GetActor(guid);
        DelActor(actor);
    }

    public Actor AddPlayer(XCharacter data, bool bRemote = false)
    {
        if (data.Id <= 0)
        {
            return null;
        }

        Actor actor = AddActor(data, true);
        actor.Add<ActorAvator>();
        actor.Add<ActorPathFinding>();
        actor.Add<ActorHUD>();
        actor.Add<ActorSkill>();
        actor.Add<ActorBuff>();
        actor.Add<ActorEffect>();
        actor.Add<ActorAnimation>();
        actor.Add<ActorCommand>();
        actor.Add<ActorMovePost>();

        //差异
        if(bRemote)
        {
            actor.Add<ActorMoveSync>();
            actor.Add<ActorAI>();
            actor.Startup(data);
            HudBlood.AddHudBlood(actor);
            actor.UpdateMountStatus();
        }
        else
        {
            actor.Startup(data);
            this.HudBlood.AddHudBlood(actor);
            Main = actor;
        }

        return actor;
    }

    public Actor AddMonster(XCharacter data)
    {
        Actor actor = AddActor(data, false);
        actor.Add<ActorAvator>();
        actor.Add<ActorPathFinding>();
        actor.Add<ActorHUD>();
        actor.Add<ActorSkill>();
		actor.Add<ActorBuff>();
        actor.Add<ActorEffect>();
        actor.Add<ActorAnimation>();
        actor.Add<ActorCommand>();
        actor.Add<ActorMovePost>();
        actor.Add<ActorMoveSync>();
        actor.Add<ActorAI>();
        actor.Startup(data);
		HudBlood.AddHudBlood (actor);
        return actor;
    }
    

    public FlyObject AddFlyObject(BulletItem data)
    {
        GameObject obj = new GameObject(string.Format("FlyObject_{0}", data.ObjectGuid));
        FlyObject flyObj = null;
        DFlyOjbect db = ReadCfgFlyObject.GetDataById(data.BulletID);
        switch (db.Type)
        {
            case EFlyObjectType.TYPE_CHASE:
                flyObj = obj.AddComponent<FlyObjectChase>();
                break;
            case EFlyObjectType.TYPE_FIXDIRECTION:
                flyObj = obj.AddComponent<FlyObjectFixDirection>();
                break;
            case EFlyObjectType.TYPE_FIXTARGETPOS:
                flyObj = obj.AddComponent<FlyObjectFixTargetPos>();
                break;
            case EFlyObjectType.TYPE_POINT:
                flyObj = obj.AddComponent<FlyObjectPoint>();
                break;
            case EFlyObjectType.TYPE_LINK:
                flyObj = obj.AddComponent<FlyObjectLink>();
                break;
            case EFlyObjectType.TYPE_ANNULAR:
                flyObj = obj.AddComponent<FlyObjectAnnular>();
                break;
            case EFlyObjectType.TYPE_BACK:
                flyObj = obj.AddComponent<FlyObjectBack>();
                break;
            case EFlyObjectType.TYPE_BOUNDCE:
                flyObj = obj.AddComponent<FlyObjectBounce>();
                break;
        }
        flyObj.Init(data);
        FlyObjects.Add(flyObj);
        return flyObj;
    }

    public FlyObject GetFlyObject(ulong guid)
    {
        for (int i = 0; i < FlyObjects.Count; i++)
        {
            if (FlyObjects[i].GUID == guid)
            {
                return FlyObjects[i];
            }
        }
        return null;
    }

    public void      DelFlyObject(ulong guid)
    {
        for (int i = 0; i < FlyObjects.Count; i++)
        {
            FlyObject fo = FlyObjects[i];
            if (fo.GUID == guid)
            {
                DelFlyObject(fo);
                break;
            }
        }
    }

    public void      DelFlyObject(FlyObject fo)
    {
        fo.Release();
        FlyObjects.Remove(fo);
        GameObject.DestroyImmediate(fo.gameObject);
    }

    public Mineral   AddMineral(XMineral data)
    {
        GameObject go = new GameObject(string.Format("Mineral_{0}", data.GUID));
        Mineral mineralObj = go.AddComponent<Mineral>();
        mineralObj.Init(data);
        return mineralObj;
    }

    public void      DelMineral(ulong guid)
    {
        for (int i = 0; i < Minerals.Count; i++)
        {
            Mineral fo = Minerals[i];
            if (fo.GUID == guid)
            {
                DelMineral(fo);
                break;
            }
        }
    }

    public void      DelMineral(Mineral fo)
    {
        GameObject.DestroyImmediate(fo.gameObject);
        Minerals.Remove(fo);
    }

    public IEnumerator AddCharactersAsync(List<XCharacter> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            XCharacter data = list[i];
            switch (data.ActorType)
            {
                case EObjectType.OT_PLAYER:
                    AddPlayer(data, data.GUID != GTData.Main.GUID);
                    break;
                case EObjectType.OT_NPC:
                case EObjectType.OT_MONSTER:
                    AddMonster(data);
                    break;
                default:
                    Debug.LogError("不正常的类型" + data.ActorType);
                    break;
            }
            yield return null;
        }
    }

    public IEnumerator AddMineralsAsync(List<XMineral> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            AddMineral(list[i]);
            yield return null;
        }
    }

    public ActorAvator AddAvatar(int modelID)
    {
        DActorModel cfg = ReadCfgActorModel.GetDataById(modelID);
        if (cfg == null)
        {
            return null;
        }
        GameObject obj = GTResourceManager.Instance.Load<GameObject>(cfg.Model, true);
        if (obj == null)
        {
            return null;
        }
        ActorAvator avatar = new ActorAvator();
        avatar.Initial(obj.transform);
        CharacterController cc = obj.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }
        return avatar;
    }

    public void SetTarget(Actor source, Actor target)
    {
        if (source == target)
        {
            return;
        }

        if (source != Main)
        {
            source.Target = target;
            return;
        }

        if(target == null)
        {
            DelTarget(source);
            return;
        }

        if (TargetCircle == null)
        {
            return;
        }

        source.Target = target;
        EShip ship = source.GetShip(target);
        Material material = TargetCircle.transform.Find("mat").GetComponent<MeshRenderer>().material;
        switch (ship)
        {
            case EShip.ES_FRIEND:
                material.SetColor("_Alpha", new Color(0, 1, 0, 0.5f));
                break;
            case EShip.ES_ENEMY:
                material.SetColor("_Alpha", new Color(1, 0, 0, 0.5f));
                break;
            case EShip.ES_NEUTRAL:
                material.SetColor("_Alpha", new Color(0, 1, 0, 0.5f));
                break;
            default:
                material.SetColor("_Alpha", new Color(1, 1, 0, 0.5f));
                break;
        }
        TargetCircle.transform.parent        = target.CacheTransform;
        TargetCircle.transform.localPosition = new Vector3(0, 0.1f, 0);

    }

    public void DelTarget(Actor source)
    {
        source.Target = null;
        if (TargetCircle != null)
        {
            TargetCircle.transform.parent = null;
            TargetCircle.transform.localPosition = new Vector3(20000, 0, 0);
        }
    }

    public void EnterWorld(int mapID)
    {
        WorldMap = new GameObject("MAPWorldMap").AddComponent<MAPWorldMap>();
        WorldMap.MapLoadFinish = OnLoadMapFinished;
        WorldMapID = mapID;
        WorldMap.EnterWorld(mapID);
    }

    public void LeaveWorld()
    {
        if (Effect != null)
        {
            Effect.Release();
        }

		GTEventCenter.FireEvent(GTEventID.TYPE_MAP_LEAVEWORLD);

		HudBlood.Clear ();

		for (int i = Minerals.Count-1; i >=0; i--)
        {
            DelMineral(Minerals[i]);
        }

		for (int i = FlyObjects.Count-1; i >=0; i--)
        {
            DelFlyObject(FlyObjects[i]);
        }

		for (int i = Characters.Count-1; i >=0 ; i--)
        {
            Actor a = Characters[i];
            if (a != GTWorld.Main)
            {
                DelActor(a);
            }
        }
    }

    public void EnterGuide()
    {
        if (Guide == null)
        {
            Guide = new GuideSystem();
            Guide.Startup();
        }
    }

    public void PauseGuide(bool pause)
    {
        if (Guide != null)
        {
            Guide.PauseGuide = pause;
        }
    }

    public void PlayCutscene(int id, Action onFinish)
    {
        if (Plot != null)
        {
            Plot.Trigger(id, () =>
            {
                GTWindowManager.Instance.HideWindow(EWindowID.UIPlotCutscene);
            });
        }
        GTWindowManager.Instance.OpenWindow(EWindowID.UIPlotCutscene);
    }

    public void SkipCutscene()
    {
        if (Plot != null)
        {
            Plot.Skip();
        }
    }

    public void Teleport(int id)
    {

    }

    public void Execute()
    {
        if (Guide != null)
        {
            Guide.Execute();
        }

        if (Plot != null)
        {
            Plot.Execute();
        }

        if (Effect != null)
        {
            Effect.Execute();
        }
    }
    void OnLoadMapFinished()
    {
		if (GTWorld.Main == null) 
		{
			if (GTData.Main.Id > 0)
			{
				GTWorld.Instance.AddPlayer (GTData.Main, false);
			}
		}
        else
        {
            GTWorld.Main.DoSyncPositionAndRotation(GTData.Main);
            GTWorld.Main.Startup(GTData.Main);
			HudBlood.AddHudBlood (GTWorld.Main);
        }

		if (GTWorld.Main != null)
		{
            GTCameraManager.Instance.CameraCtrl.SetTarget(Main.CacheTransform, Main.Height * 0.5f);
            GTNetworkSend.  Instance.TrySyncIdle();
            GTEventCenter.FireEvent(GTEventID.TYPE_MAP_ENTERWORLD);
        }

        if (GTLauncher.Instance.CurScene != null)
        {
            GTLauncher.Instance.CurScene.OpenWindows();
        }
    }
}
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Protocol;
using ACT;
using DG.Tweening;
using UnityEngine.AI;

[DefaultExecutionOrder(900)]
public class Actor : MonoBehaviour, IActor
{
    public ulong                     GUID            { get; set; }
    public Int32                     ID              { get; set; }
    public GameObject                Obj             { get; private set; }
    public string                    Name            { get; private set; }
    public Int32                     Camp            { get; private set; }
    public EObjectType               Type            { get; private set; }
    public DActor                    DBActor         { get; private set; }
    public DActorModel               DBModel         { get; private set; }
    public Int32                     Level           { get; set; }
    public Int32                     Title           { get; private set; }
    public Int32                     ActorStatus     { get; set; }
    public Int32                     Team            { get; private set; }
    public Int32                     Group           { get; private set; }
    public Int32                     Guild           { get; private set; }
    public int                       Carrer          { get; private set; }
    public ActorAttr                 Attr            { get; private set; }
    public Actor                     Attker          { get; set; }
    public Actor                     Target          { get; set; }
    public int                       MountID         { get; set; }
    public Actor                     MountActor      { get; set; }
    public bool                      IsRide          { get { return MountID > 0; }  }
    public Callback                  OnDeadFinished  { get; set; }
    public bool                      IsNativeActor   { get; set; }
    public bool                      IsStealth       { get; set; }
    public bool                      IsFixBody       { get; set; }
    public bool                      IsNavigate      { get; set; }
    public bool                      IsLoad          { get; private set; }
    public ulong                     ControlID       { get;  set; }
    public ulong                     HostID          { get; set; }

    public Actor Host
    {
        get
        {
            return  GTWorld.Instance.GetActor(HostID);
        }
    }
    public Transform                 CacheTransform
    {
        get { return MountActor != null ? MountActor.CacheTransform : transform; }
    }

    public Vector3                   Pos
    {
        get { return CacheTransform.position; }
        set { CacheTransform.position = value; }
    }

    public Vector3                   Euler
    {
        get { return CacheTransform.eulerAngles; }
        set { CacheTransform.eulerAngles = value; }
    }

    public Vector3                   Scale
    {
        get { return CacheTransform.localScale; }
        set { CacheTransform.localScale = value; }
    }

    public float                     Face
    {
        get { return Euler.y; }
        set { Euler = new Vector3(0, value, 0); }
    }

    public Vector3                   Dir
    {
        get { return CacheTransform.forward; }
        set { CacheTransform.forward = value; }
    }           

    public CharacterController       Ctrl
    {
        get;
        private set;
    }
    public ETransform                BornData
    {
        get;
        set;
    }

    public FSMState                  FSM
    {
        get; private set;
    }

    public float                     Height
    {
        get {
            if(IsRide)
            {
               return (GetComponent<CharacterController>().height + Ctrl.height)* CacheTransform.localScale.y;
            }
            return Ctrl == null ? 1 : Ctrl.height * CacheTransform.localScale.y;
        }
    }

    public float                     Radius
    {
        get { return Ctrl == null ? 1 : Ctrl.radius * CacheTransform.localScale.x; }
    }

    public Vector3                   Center
    {
        get { return Ctrl == null ? Vector3.zero : Ctrl.center; }
    }

    public float                     CurSpeed
    {
        get
        {
            return Attr == null ? 0 : DBActor.DefSpeed * Attr.GetAttr(EAttrID.EA_SPEED) / 10000.0f;
        }
    }

	
    public List<IActorComponent>     Components
    {
        get { return m_Components; }
    }

    public List<SkillItem>     Skills
    {
        get { return m_Skills; }
    }

    private List<IActorComponent>          m_Components    = new List<IActorComponent>();
    private List<Actor>                    m_Targets       = new List<Actor>();
    private List<bool>                     m_Restricts     = new List<bool>();

    private List<SkillItem> m_Skills = new List<SkillItem>();

    public void Initial(XCharacter data)
    {
        this.GUID     = data.GUID;
        this.Type     = data.ActorType;
        this.Camp     = data.Camp;
        this.Level    = data.Level;
        this.Title    = data.Title;
        this.Group    = data.Group;
        this.Name     = data.Name;
        this.Carrer   = data.Carrer;
        this.m_Skills = data.CurSkills;
        this.MountID = data.Mount;
        this.ControlID = data.ControlID;
        this.HostID = data.HostID;
        this.InitGameObject(data.Id);
        this.InitAttr(data);
        this.InitCollinder();
        this.InitRestricts();
        this.BornData = ETransform.Create(data.PosX, data.PosY, data.PosZ, data.Face);
        this.Pos      = BornData.Pos;
        this.Euler    = BornData.Euler;
    }

    public void UpdateMountStatus()
    {
        if(MountID < 0)
        {
            DoRideOff();
        }
        else
        {
            DoRideOn();
        }
    }
    public void CreateMountActor()
    {
        DMount dbMount = ReadCfgMount.GetDataById(MountID);

        if (MountActor != null)
        {
            if(dbMount.ActorId != MountActor.ID)
            {
                DestroyMountActor();
            }
            else
            {
                return;
            }
        }

        XCharacter data = new XCharacter();

        data.Id = dbMount.ActorId;

        data.GUID = GTData.NewGUID;

        data.ActorType = EObjectType.OT_MOUNT;

        GameObject g = new GameObject(string.Format("Actor {0} {1}", data.Id.ToString("000000"), data.GUID));

        MountActor = g.AddComponent<Actor>();

        MountActor.Initial(data);

        MountActor.Add<ActorAvator>();

        MountActor.Add<ActorAnimation>();

        g.transform.parent = transform.parent;
        g.transform.position = GTTools.GroundPosition(transform.position, 0);
        this.transform.parent = MountActor.transform;
        this.transform.localPosition = new Vector3(0,2,0.4f);
        this.transform.localEulerAngles = Vector3.zero;
        this.transform.localScale = Vector3.one;

        Ctrl.enabled = false;
        Ctrl = MountActor.GetComponent<CharacterController>();
        Ctrl.enabled = true;

        return;
    }

    public void DestroyMountActor()
    {
        if(MountActor == null)
        {
            return;
        }

        this.transform.parent = MountActor.transform.parent;
        this.transform.position = MountActor.transform.position;
        Ctrl = GetComponent<CharacterController>();
        Ctrl.enabled = true;


        GTPoolManager.Instance.ReleaseGo(MountActor.DBModel.Model, MountActor.Obj);
        GameObject.Destroy(MountActor.gameObject);
        UnityEngine.Object.Destroy(MountActor);
        MountActor = null;

        return;
    }

    public void Startup(XCharacter data)
    {
        this.ShowHUD(true);

        this.ShowAvatar(data);

        if(data.ActionID == EActionType.AT_IDLE)
        {
            DoIdle();
        }
        else if (data.ActionID == EActionType.AT_WALK)
        {
            DoWalk();
        }
        else if (data.ActionID == EActionType.AT_RUN)
        {
            DoRun();
        }
    }

    void InitGameObject(int id)
    {
        this.ID      = id;
        this.DBActor = ReadCfgActor.GetDataById(id);
        this.DBModel = ReadCfgActorModel.GetDataById(this.DBActor.Model);
        this.Scale   = Vector3.one * this.DBModel.ModelScale;
        this.Obj     = GTPoolManager.Instance.GetObject(this.DBModel.Model + ".prefab");
        this.Obj.name = "Body";
        this.Obj.transform.parent = CacheTransform;
        this.Obj.transform.localPosition = Vector3.zero;
        this.Obj.transform.localEulerAngles = Vector3.zero;
        this.Obj.transform.localScale = Vector3.one;
        this.IsLoad = true;
        switch(Type)
        {
            case EObjectType.OT_PLAYER:
                NGUITools.SetLayer(gameObject, GTLayer.LAYER_PLAYER);
                break;
            case EObjectType.OT_PET:
            case EObjectType.OT_PARTNER:
            case EObjectType.OT_MOUNT:
                NGUITools.SetLayer(gameObject, GTLayer.LAYER_PET);
                break;
            case EObjectType.OT_MONSTER:
                NGUITools.SetLayer(gameObject, GTLayer.LAYER_MONSTER);
                break;
            default:
                NGUITools.SetLayer(gameObject, GTLayer.LAYER_ACTOR);
                break;
        }
    }

    void InitAttr(XCharacter data)
    {
        this.Attr = new ActorAttr();
        this.Attr.Update(data == null ? null : data.CurAttrs);
    }

    void InitCollinder()
    {
        CharacterController ctrl = Obj.GetComponent<CharacterController>();
        ctrl.enabled             = false;
        this.Ctrl = this.CacheTransform.gameObject.GET<CharacterController>();
        this.Ctrl.center = ctrl.center;
        this.Ctrl.radius = ctrl.radius;
        this.Ctrl.height = ctrl.height;
        this.Ctrl.enabled = true;
    }

    void InitRestricts()
    {
        this.m_Restricts = GTTools.GetListFromEnumNames<bool>(typeof(EActorNature));
        for (int i = 0; i < DBActor.Natures.Count; i++)
        {
            this.m_Restricts[i] = DBActor.Natures[i];
        }
    }

    void Update()
    {
        for (int i = 0; i < m_Components.Count; i++)
        {
            IActorComponent c = m_Components[i];
            c.Execute();
        }
    }

    public T           Add<T>() where T : IActorComponent
    {
        T component = System.Activator.CreateInstance<T>();
        component.Initial(this);
        m_Components.Add(component);
        return component;
    }

    public T           Del<T>(T t) where T : IActorComponent
    {
        if (t != null)
        {
            for (int i = m_Components.Count - 1; i >= 0; i--)
            {
                if (m_Components[i].Equals(t))
                {
                    m_Components.RemoveAt(i);
                    break;
                }
            }
            return default(T);
        }
        else
        {
            return default(T);
        }

    }

    public T           Get<T>() where T : IActorComponent
    {
        for (int i = 0; i < m_Components.Count; i++)
        {
            if(m_Components[i] is T)
            {
                return (T)m_Components[i];
            }
        }
        return default(T);
    }

    public void        CheckMoveEndPointInFront(ref Vector3 endValue, float maxDir)
    {
        Vector3 rayStartPoint = Pos + new Vector3(0, Height / 2, 0);
        RaycastHit hitInfo;
        if (Physics.Raycast(rayStartPoint, Dir, out hitInfo, maxDir, LayerMask.GetMask("Default")))
        {
            endValue = hitInfo.point;
            endValue.y = Pos.y;
            endValue -= Radius * Dir;
        }
    }

    public void        CheckMoveEndPointInBack(ref Vector3 endValue, float maxDir)
    {
        Vector3 rayStartPoint = Pos + new Vector3(0, Height / 2, 0);
        RaycastHit hitInfo;
        if (Physics.Raycast(rayStartPoint, Dir, out hitInfo, maxDir, LayerMask.GetMask("Default")))
        {
            endValue = hitInfo.point;
            endValue.y = Pos.y;
            endValue += Radius * Dir;
        }
    }

    public List<Actor> GetAllys()
    {
        m_Targets.Clear();
        GetActorsByShip(EShip.ES_FRIEND, ref m_Targets);
        return m_Targets;
    }

    public List<Actor> GetEnemys()
    {
        m_Targets.Clear();
        GetActorsByShip(EShip.ES_ENEMY, ref m_Targets);
        return m_Targets;
    }

    public List<Actor> GetAffectTargets(EAffect affect, ESelectTargetPolicy policy)
    {
        m_Targets.Clear();
        for (int i = 0; i < GTWorld.Characters.Count; i++)
        {
            Actor actor = GTWorld.Characters[i];
            if (Match(affect, actor) == false)
            {
                continue;
            }
            if (actor.CanBeSearch() != Resp.TYPE_YES)
            {
                continue;
            }
            m_Targets.Add(actor);
        }
        switch(policy)
        {
            case ESelectTargetPolicy.TYPE_SELECT_DEFAULT:
                break;
            case ESelectTargetPolicy.TYPE_SELECT_BY_MOREHEALTH:
                if (m_Targets.Count > 0)
                {
                    m_Targets.Sort((a, b) =>
                    {
                        float a1 = a.Attr.GetAttr(EAttrID.EA_HP) * 1f / a.Attr.GetAttr(EAttrID.EA_HP_MAX);
                        float a2 = b.Attr.GetAttr(EAttrID.EA_HP) * 1f / b.Attr.GetAttr(EAttrID.EA_HP_MAX);
                        float dd = a1 - a2;
                        return (dd < 0) ? 1 : ((dd > 0) ? 0 : -1);
                    });
                }
                break;
            case ESelectTargetPolicy.TYPE_SELECT_BY_LESSHEALTH:
                if (m_Targets.Count > 0)
                {
                    m_Targets.Sort((a, b) =>
                    {
                        float a1 = a.Attr.GetAttr(EAttrID.EA_HP) * 1f / a.Attr.GetAttr(EAttrID.EA_HP_MAX);
                        float a2 = b.Attr.GetAttr(EAttrID.EA_HP) * 1f / b.Attr.GetAttr(EAttrID.EA_HP_MAX);
                        float dd = a1 - a2;
                        return (dd < 0) ? -1 : ((dd > 0) ? 0 : 1);
                    });
                }
                break;
            case ESelectTargetPolicy.TYPE_SELECT_BY_MOREDISTANCE:
                if (m_Targets.Count > 0)
                {
                    m_Targets.Sort((a, b) =>
                    {
                        float a1 = GTTools.GetHorizontalDistance(a.Pos, this.Pos);
                        float a2 = GTTools.GetHorizontalDistance(b.Pos, this.Pos);
                        float dd = a1 - a2;
                        return (dd < 0) ? 1 : ((dd > 0) ? 0 : -1);
                    });
                }
                break;
            case ESelectTargetPolicy.TYPE_SELECT_BY_LESSDISTANCE:
                if (m_Targets.Count > 0)
                {
                    m_Targets.Sort((a, b) =>
                    {
                        float a1 = GTTools.GetHorizontalDistance(a.Pos, this.Pos);
                        float a2 = GTTools.GetHorizontalDistance(b.Pos, this.Pos);
                        float dd = a1 - a2;
                        return (dd < 0) ? -1 : ((dd > 0) ? 0 : 1);
                    });
                }
                break;
        }
        return m_Targets;
    }

    public Actor       GetTargetByPolicy(EAffect affect, ESelectTargetPolicy policy)
    {
        List<Actor> list = GetAffectTargets(affect, policy);
        return list.Count > 0 ? list[0] : null;
    }

    public int       GetCurrentHP()
    {
        return Attr.GetAttr(EAttrID.EA_HP);
    }

    public int       GetCurrentMP()
    {
        return Attr.GetAttr(EAttrID.EA_MP);

    }

    public int       GetMaxHP()
    {
        return Attr.GetAttr(EAttrID.EA_HP_MAX);
    }

    public int       GetMaxMP()
    {
        return Attr.GetAttr(EAttrID.EA_MP_MAX);
    }

    public float       GetHpPercent()
    {
        return GetCurrentHP() / GetMaxHP();
    }

    public float       GetMpPercent()
    {
        return GetCurrentMP() / GetMaxMP();
    }

    public bool        Match(EAffect affect, Actor actor)
    {
        if (actor == null)
        {
            return false;
        }

        if (actor.IsDead())
        {
            return false;
        }

        switch(affect)
        {
            case EAffect.Self:
                return actor == this;
            case EAffect.Lock:
                return actor == Target;
            case EAffect.Host:
                return actor == Host;
            case EAffect.ESef:
                return actor != this;
            case EAffect.Atkr:
                return actor == Attker;
            case EAffect.Enem:
                return GetShip(actor) == EShip.ES_ENEMY;
            case EAffect.Ally:
                return GetShip(actor) == EShip.ES_FRIEND;
            case EAffect.Team:
                return actor.Team == this.Team;
            case EAffect.Tuan:
                return actor.Group == this.Group;
            case EAffect.Guid:
                return actor.Guild == this.Guild;
            case EAffect.Camp:
                return false;
            case EAffect.Each:
                return true;
            default:
                return false;
        }
    }

    public bool        MatchCanAttack(Actor actor)
    {
        return Match(EAffect.Enem, actor);
    }

    public EShip        GetShip(Actor actor)
    {
        if (actor == null)
        {
            return EShip.ES_NEUTRAL;
        }
        if (Camp == 0)
        {
            return EShip.ES_NEUTRAL;
        }

        if (actor.Camp == Camp)
        {
            return EShip.ES_FRIEND;
        }

        return  EShip.ES_ENEMY;
    }

    public bool        GetFeatures(EActorNature type)
    {
        int index = (int)type;
        return DBActor.Natures[index];
    }

    public void        GetActorsByShip(EShip relationShip, ref List<Actor> list)
    {
        for (int i = 0; i < GTWorld.Characters.Count; i++)
        {
            Actor actor = GTWorld.Characters[i];
            if (GetShip(actor) == relationShip)
            {
                list.Add(actor);
            }
        }
    }

    public bool        GetRestrict(EActorNature type)
    {
        return m_Restricts[(int)type];
    }

    public void        SetRestrict(EActorNature type, bool restrict)
    {
        m_Restricts[(int)type] = restrict;
    }

    public void        FaceTarget(float duration)
    {
        if (Target != null)
        {
            if (duration > 0)
            {
                Vector3 towards = Target.Pos - Pos;
                towards.Normalize();
                CacheTransform.DOLookAt(towards, duration, AxisConstraint.Y);
            }
            else
            {
                Vector3 point = new Vector3(Target.Pos.x, Pos.y, Target.Pos.z);
                CacheTransform.LookAt(point);
            }
        }
    }

    public void        EnableCharacter(bool enabled)
    {
        if (Ctrl != null && Ctrl.enabled != enabled)
        {
            Ctrl.enabled = enabled;
        }
    }

    public void        PlayPathFinding(Vector3 destination, Callback onFinish)
    {
        ActorPathFinding c = Get<ActorPathFinding>();
        if (c == null)
        {
            return;
        }

        onFinish += StopNavigate;
        this.IsNavigate = true;
        c.SetSpeed(CurSpeed);
        c.SetDestination(destination, onFinish);
    }

    public void        StopPathFinding()
    {
        ActorPathFinding c = Get<ActorPathFinding>();
        if (c != null)
        {
            c.Stop();
        }
    }

    public void        StopNavigate()
    {
        if(this.IsNavigate)
        {
            this.StopPathFinding();

            this.IsNavigate = false;
        }
    }

    public void        PlayAnim(string animName, Callback onFinish = null, bool isLoop = false, float speed = 1f, float lastTime = 0f)
    {
        if(MountActor != null)
        {
            ActorAnimation c1 = Get<ActorAnimation>();
            if (c1 != null)
            {
                c1.Play("qicheng_" + animName, onFinish, isLoop, speed, lastTime);
            }

            ActorAnimation c2 = MountActor.Get<ActorAnimation>();
            if (c2 != null)
            {
                c2.Play(animName, onFinish, isLoop, speed, lastTime);
            }
        }
        else
        {
            ActorAnimation c1 = Get<ActorAnimation>();
            if (c1 != null)
            {
                c1.Play(animName, onFinish, isLoop, speed, lastTime);
            }
        }
    }

    public void        ShowAvatar(XCharacter data)
    {
        if (data == null || data.CurEquips.Count != 8)
        {
            return;
        }
        ActorAvator actorAvatar = Get<ActorAvator>();
        if (actorAvatar == null)
        {
            return;
        }
        for (int i = 0; i < data.CurEquips.Count; i++)
        {
            actorAvatar.ChangeAvatar(i + 1, data.CurEquips[i]);
        }
    }

    public void        ShowHUD(bool active)
    {
        ActorHUD c = Get<ActorHUD>();
        if (c == null)
        {
            return;
        }

        if (!c.IsOk())
        {
            c.Initial(this);
        }
       

        if (active)
        {
            c.SetHeight(Height);
            if (IsNativeActor)
            {
                c.Show(string.Format("[00ffff]{0}[-]", "暮光审判"), string.Format("[00ff00]{0}[-]", Name));
            }
            else
            {
                EShip ship = GetShip(GTWorld.Main);
                switch (ship)
                {
                    case EShip.ES_FRIEND:
                        c.Show(string.Format("[00ff00]{0}[-]", Name), string.Empty);
                        break;
                    case EShip.ES_ENEMY:
                        c.Show(string.Format("[ff0000]{0}[-]", Name), string.Empty);
                        break;
                    case EShip.ES_NEUTRAL:
                        c.Show(string.Format("[00ffff]{0}[-]", Name), string.Empty);
                        break;
                }
            }
        }
        else
        {
            c.SetVisable(false);
        }
    }

    public void        ShowFlyWord(int chgHp, bool bCrit)
    {
        Vector3 WordPos = Pos + Vector3.up * Height * 0.5f;

        EFlyWordType tFlyType = EFlyWordType.TYPE_NONE;


        if (IsNativeActor)
        {
            if (chgHp > 0)
            {
                tFlyType = bCrit ? EFlyWordType.TYPE_HOST_HEAL_CRIT : EFlyWordType.TYPE_HOST_HEAL_NORM;
            }
            else if(chgHp == 0)
            {
                tFlyType = EFlyWordType.TYPE_HOST_DODGE;
            }
            else
            {
                tFlyType = bCrit ? EFlyWordType.TYPE_HOST_HURT_CRIT : EFlyWordType.TYPE_HOST_HURT_NORM;
            }
        }
        else
        {
            if (chgHp > 0)
            {
                tFlyType = bCrit ? EFlyWordType.TYPE_OTHER_HEAL_CRIT : EFlyWordType.TYPE_OTHER_HEAL_NORM;
            }
            else if (chgHp == 0)
            {
                tFlyType = EFlyWordType.TYPE_OTHER_DODGE;
            }
            else
            {
                tFlyType = bCrit ? EFlyWordType.TYPE_OTHER_HURT_CRIT : EFlyWordType.TYPE_OTHER_HURT_NORM;
            }
        }

        GTWorld.Instance.Flyword.Play(chgHp, WordPos, tFlyType);
    }

    public Resp        CanUseSkill(ActSkill skill)
    {
        if (skill == null)
        {
            return Resp.TYPE_SKILL_NOTFIND;
        }

        if (skill.IsCD())
        {
            return Resp.TYPE_SKILL_CD;
        }

        if (skill.IsAutoLockTarget())
        {
            if (Target != null)
            {
                if (MatchCanAttack(Target) == false)
                {
                    return Resp.TYPE_SKILL_CANNOT_ATTACK_TARGET;
                }
                Vector3 dist = Target.Pos - Pos;
                dist.y = 0;
                if (dist.sqrMagnitude > skill.CastDistance * skill.CastDistance)
                {
                    return Resp.TYPE_SKILL_TOOFAR;
                }
            }
            else
            {
                return Resp.TYPE_SKILL_TARGET_NULL;
            }
        }
        return Resp.TYPE_YES;
    }

    public Resp        CanUseSkillForFSM()
    {
        if (IsFSMLayer2())
        {
            return Resp.TYPE_ACTOR_FSMLAYER3;
        }
        if (IsFSMLayer1())
        {
            return Resp.TYPE_ACTOR_FSMLAYER2;
        }
        if (IsCastingSkill())
        {
            return Resp.TYPE_SKILL_CASTING;
        }
        return Resp.TYPE_YES;
    }

    public Resp        CanBeAttack(bool ignoreStealth)
    {
        if (IsDead())
        {
            return Resp.TYPE_ACTOR_DEAD;
        }
        if (IsStealth && ignoreStealth)
        {
            return Resp.TYPE_SKILL_CANNOT_ATTACK_WITH_STEALTH;
        }
        if (GetFeatures(EActorNature.CAN_KILL))
        {
            return Resp.TYPE_ACTOR_CANNOTBE_ATTACK;
        }
        return Resp.TYPE_YES;
    }

    public Resp        CanBeSearch()
    {
        if (IsDead())
        {
            return Resp.TYPE_ACTOR_DEAD;
        }
        if (IsStealth)
        {
            return Resp.TYPE_SKILL_CANNOT_ATTACK_WITH_STEALTH;
        }
        if (GetFeatures(EActorNature.CAN_KILL) == false)
        {
            return Resp.TYPE_ACTOR_CANNOTBE_ATTACK;
        }
        return Resp.TYPE_YES;
    }

    public bool        CanMove()
    {
        if (GetFeatures(EActorNature.CAN_MOVE) == false)
        {
            return false;
        }
        if (IsFSMLayer1() || IsFSMLayer2())
        {
            return false;
        }

        if (FSM == FSMState.FSM_FIXBODY)
        {
            return false;
        }

        ActorSkill actorSkill = Get<ActorSkill>();
        if (actorSkill == null)
        {
            return true;
        }
        if (actorSkill.GetCurrent() == null)
        {
            return true;
        }
        if (actorSkill.GetCurrent().CanMove())
        {
            return true;
        }
        return false;
    }

    public bool        CanTurn()
    {
        if (GetFeatures(EActorNature.CAN_TURN) == false)
        {
            return false;
        }
        if (IsFSMLayer1() || IsFSMLayer2())
        {
            return false;
        }
        ActorSkill actorSkill = Get<ActorSkill>();
        if (actorSkill == null)
        {
            return true;
        }
        if (actorSkill.GetCurrent() == null)
        {
            return true;
        }
        if (actorSkill.GetCurrent().CanTurn())
        {
            return true;
        }
        return false;
    }

    public bool        IsDead()
    {
        return FSM == FSMState.FSM_DEAD;
    }

    public bool        IsMove()
    {
        return FSM == FSMState.FSM_WALK || FSM == FSMState.FSM_RUN || FSM == FSMState.FSM_FLY;
    }

    public bool        IsIdle()
    {
        return FSM == FSMState.FSM_IDLE;
    }

    public bool        IsCastingSkill()
    {
        return FSM == FSMState.FSM_SKILL || FSM == FSMState.FSM_PRE_SKILL;
    }

    public bool        IsFSMLayer1()
    {
        switch (FSM)
        {
            case FSMState.FSM_MINE:
            case FSMState.FSM_ROLL:
            case FSMState.FSM_JUMP:
            case FSMState.FSM_DANCE:
                return true;
            default:
                return false;
        }
    }

    public bool        IsFSMLayer2()
    {
        switch (FSM)
        {
            case FSMState.FSM_BEATFLY:
            case FSMState.FSM_BEATDOWN:
            case FSMState.FSM_BEATBACK:
            case FSMState.FSM_DEAD:
            case FSMState.FSM_FLOATING:
            case FSMState.FSM_WOUND:
                return true;
            default:
                return false;
        }
    }


    public void DoWalk()
    {
        this.FSM = FSMState.FSM_WALK;
        PlayAnim("walk", null, true);
    }

    public void DoRun()
    {
        this.FSM = FSMState.FSM_RUN;
        PlayAnim("run", null, true);
    }

    public void DoFly()
    {
        this.FSM = FSMState.FSM_RUN;
        PlayAnim("fly", null, true);
    }

    public void        DoIdle()
    {
        StopNavigate();
        this.FSM = FSMState.FSM_IDLE;
        this.PlayAnim("idle", null, true);
    }

    public void DoPreSkill()
    {
        StopNavigate();
        this.FSM = FSMState.FSM_PRE_SKILL;
        this.PlayAnim("idle", null, true);
    }


    public void        DoSkill(int skillID)
    {
        ActorSkill c = Get<ActorSkill>();
        if (c == null)
        {
            Debug.LogError("actor have no ActorSkill can not player skillid" + skillID);
            return;
        }

        StopNavigate();
        if (c.PlaySkill(skillID, DoIdle))
        {
            this.FSM = FSMState.FSM_SKILL;
            this.EnableCharacter(true);
        }
    }

    public void        DoJump()
    {
        StopNavigate();
        PlayAnim("jump", DoIdle);
    }

    public void        DoDead(EDeadReason reason)
    {
        this.FSM = FSMState.FSM_DEAD;
        StopNavigate();
        this.EnableCharacter(false);
        ActorSkill aSkill = Get<ActorSkill>();
        if (aSkill != null)
        {
            aSkill.Release();
        }
        ActorBuff  aBuff = Get<ActorBuff>();
        if (aBuff != null)
        {
            aBuff.Release();
        }
        ActorAvator aAvator = Get<ActorAvator>();
        if (aAvator != null)
        {
            aAvator.SetShadowActive(false);
        }
        
        if (OnDeadFinished != null)
        {
            this.PlayAnim("die", OnDeadFinished);
        }
        else
        {
            this.PlayAnim("die", OnDeadFinishedDef);
        }
    }

    public void        OnDeadFinishedDef()
    {
        GTWorld.Instance.DelActor(this);
    }

    public void DoMoveForward()
    {
        this.StopPathFinding();
        if (CanMove())
        {
            this.EnableCharacter(true);
            this.Ctrl.SimpleMove(Dir * CurSpeed);
        }
    }

    public void        DoSimpleMove(Vector3 motion)
    {
        this.StopPathFinding();
        if (CanTurn())
        {
            this.CacheTransform.LookAt(Pos + motion);
        }
        if (CanMove())
        {
            this.EnableCharacter(true);
            this.Ctrl.SimpleMove(Dir * CurSpeed);
            if (IsCastingSkill() == false)
            {
                this.FSM = FSMState.FSM_RUN;
                this.PlayAnim("run", null, true);
            }
        }
    }

    public void        DoNavigateToDestination(Vector3 destination, Callback reachCallback)
    {
        if (CanTurn())
        {
            Vector3 pos = destination;
            pos.y = CacheTransform.position.y;
            CacheTransform.DOLookAt(pos, 0.2f);
        }

        if (CanMove())
        {
            ActorPathFinding c = Get<ActorPathFinding>();
            if (c == null)
            {
                return;
            }

            this.EnableCharacter(false);

            this.PlayPathFinding(destination, reachCallback);

            if (IsCastingSkill() == false)
            {
                this.FSM = FSMState.FSM_RUN;
                this.PlayAnim("run", null, true);
            }  
        }
    }

    public void        DoNavigateToTarget(Vector3 target, Callback reachCallback)
    {
        DoNavigateToDestination(target, reachCallback);
    }

	public void        DoActorHitEffect(HitEffectItem data)
    {
        ShowFlyWord(data.HurtValue, data.Crit);

        if (IsDead())
        {
            return;
        }

       if((Type == EObjectType.OT_PLAYER)&&(FSM != FSMState.FSM_IDLE))
        {
            return;
        }

        Transform bindTransform = this.Get<ActorAvator>().GetBindTransform(EBind.Body);
        ActorEffect effectMgr = this.Get<ActorEffect>();
        effectMgr.AddEffect((int)data.HitEffectID, bindTransform, 2.0f, false);

        switch ((EActionType)data.HitActionID)
        {
            case 0:
            case EActionType.AT_WOUND:
                {
                    DoHit();
                }
                break;

            case EActionType.AT_BEATBACK:
                {
                    DoBeatBack(data.HitDistance, data.CastGuid);
                }
                break;

            case EActionType.AT_BEATDOWN:
                {
                    DoBeatDown(data.CastGuid);
                }
                break;

            case EActionType.AT_BEATFLY:
                {
                    DoBeatFly(data.HitDistance, data.CastGuid);
                }
                break;
        }
    }

    public void DoBeatDown(ulong uCastGuid)
    {
        this.StopNavigate();

        this.PlayAnim("fei", DoIdle);

        this.FSM = FSMState.FSM_BEATBACK;
    }

    public void        DoBeatFly(float maxDis, ulong uCastGuid)
    {
        this.StopNavigate();
        this.EnableCharacter(true);
        Actor CastActor = GTWorld.Instance.GetActor(uCastGuid);
        Vector3 towards;
        if (CastActor.Pos == Pos)
        {
            towards = CastActor.Dir;
            towards.x = -towards.x;
            towards.z = -towards.z;
        }
        else
        {
            towards = CastActor.Pos - Pos;
        }

        towards.Normalize();

        CacheTransform.DOLookAt(towards, 0.2f, AxisConstraint.Y);
        ActorPathFinding c = Get<ActorPathFinding>();
        if (c == null)
        {
            return;
        }

        this.EnableCharacter(false);

        this.IsNavigate = true;

        c.SetSpeed(30.0f);

        towards.x = -towards.x;

        towards.z = -towards.z;

        c.SetDestination(towards * maxDis + Pos, StopNavigate);
        this.PlayAnim("fly", DoIdle);
        this.FSM = FSMState.FSM_BEATFLY;
    }

    public void DoBeatBack(float maxDis, ulong uCastGuid)
    {
        this.StopNavigate();
        this.EnableCharacter(true);
        Actor CastActor = GTWorld.Instance.GetActor(uCastGuid);
        Vector3 towards;
        if (CastActor.Pos == Pos)
        {
            towards = CastActor.Dir;
            towards.x = -towards.x;
            towards.z = -towards.z;
        }
        else
        {
            towards = CastActor.Pos - Pos;
        }

        towards.Normalize();

        CacheTransform.DOLookAt(towards, 0.2f, AxisConstraint.Y);
        ActorPathFinding c = Get<ActorPathFinding>();
        if (c == null)
        {
            return;
        }

        this.EnableCharacter(false);

        this.IsNavigate = true;

        c.SetSpeed(30.0f);

        towards.x = -towards.x;

        towards.z = -towards.z;

        NavMeshHit hit  = CastActor.Get<ActorPathFinding>().GetRaycastHit(towards * maxDis + Pos);
        if(hit.hit)
        {
            c.SetDestination(hit.position, StopNavigate);
        }
        else
        {
            c.SetDestination(towards * maxDis + Pos, StopNavigate);
        }

        this.PlayAnim("fei", DoIdle);

        this.FSM = FSMState.FSM_BEATFLY;
    }

    public void        DoFixBody()
    {
        this.StopNavigate();
        this.EnableCharacter(true);
        this.PlayAnim("yun", null, true);
        this.FSM = FSMState.FSM_FIXBODY;
    }

    public void        DoHit()
    {
        this.StopNavigate();
        this.EnableCharacter(true);
        this.PlayAnim("hit", DoIdle);
        this.FSM = FSMState.FSM_WOUND;
    }

    public void DoRideOn()
    {
        if(MountID <= 0)
        {
            return;
        }

        this.StopNavigate();

        CreateMountActor();

        this.PlayAnim("idle", null, true);

        this.FSM = FSMState.FSM_IDLE;
    }

    public void DoAction(FSMState sMState)
    {
        switch (sMState)
        {
            //==========需要同步的状态============
            case FSMState.FSM_IDLE:
                {
                    DoIdle();
                }
                break;
            case FSMState.FSM_FIXBODY:             //定身
                {
                    DoFixBody();
                }
                break;
            case FSMState.FSM_WALK:                //行走
                {
                    DoWalk();
                }
                break;
            case FSMState.FSM_RUN:                //跑动
                {
                    DoRun();
                }
                break;
            case FSMState.FSM_FLY:                 //飞行
                {
                    DoFly();
                }
                break;
            //=========主动操作的状态=============
            case FSMState.FSM_SKILL:               //技能
                {

                }
                break;
            case FSMState.FSM_MINE:                //采集
                {

                }
                break;
            case FSMState.FSM_ROLL:                //翻滚
                {

                }
                break;
            case FSMState.FSM_JUMP:                //跳跃
                {

                }
                break;
            case FSMState.FSM_BORN:                //出生
                {

                }
                break;
            case FSMState.FSM_DANCE:               //跳舞
                {

                }
                break;

            //=========被动状态=================
            case FSMState.FSM_DEAD:               //死亡
                {

                }
                break;
            case FSMState.FSM_WOUND:               //受击
                {

                }
                break;
            case FSMState.FSM_BEATBACK:            //击退
                {

                }
                break;
            case FSMState.FSM_BEATDOWN:           //击倒
                {

                }
                break;
            case FSMState.FSM_BEATFLY:            //击飞
                {

                }
                break;
            case FSMState.FSM_FLOATING:            //浮空
                {

                }
                break;
            default:
                {

                }
                break;
        }

    }

    public void DoRideOff()
    {
        this.StopNavigate();

        DestroyMountActor();

        DoIdle();
        this.FSM = FSMState.FSM_IDLE;
    }

    public void        DoReborn()
    {
        this.StopNavigate();
        this.EnableCharacter(true);
        this.PlayAnim("fuhuo", DoIdle);
        this.FSM = FSMState.FSM_BORN;
    }

    public void        DoDance(int type)
    {
        this.StopNavigate();
        this.EnableCharacter(true);
        switch (type)
        {
            case 1:
                this.FSM = FSMState.FSM_DANCE;
                this.PlayAnim("dance1", DoIdle);
                break;
            case 2:
                this.FSM = FSMState.FSM_DANCE;
                this.PlayAnim("dance2", DoIdle);
                break;
            case 3:
                this.FSM = FSMState.FSM_DANCE;
                this.PlayAnim("dance3", DoIdle);
                break;
        }
    }

    public void        DoSyncPositionAndRotation(XCharacter data)
    {
        this.BornData = ETransform.Create(data.PosX, data.PosY, data.PosZ, data.Face);
        this.Pos      = BornData.Pos;
        this.Euler    = BornData.Euler;
    }
}
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using ACT;
using Protocol;

public class ActorCommand : IActorComponent
{
    protected Actor                      m_Actor;
    protected ActorSkill                 m_ActorSkill;
    protected ActorPathFinding           m_ActorPathFinding;
    protected Dictionary<Type, ICommand> m_Cmds             = new Dictionary<Type, ICommand>();

    public void Initial(Actor actor)
    {
        this.m_Actor            = actor;
        this.m_ActorSkill       = actor.Get<ActorSkill>();
        this.m_ActorPathFinding = actor.Get<ActorPathFinding>();
        this.InitCommands();
    }
    public void Execute()
    {

    }

    public void Release()
    {

    }

    void InitCommands()
    {
        this.AddCmd<CommandIdle>(Check);
        this.AddCmd<CommandMove>(Check);
        this.AddCmd<CommandDead>(Check);
        this.AddCmd<CommandTalk>(Check);
        this.AddCmd<CommandFrost>(Check);
        this.AddCmd<CommandStun>(Check);
        this.AddCmd<CommandParaly>(Check);
        this.AddCmd<CommandSleep>(Check);
        this.AddCmd<CommandBlind>(Check);
        this.AddCmd<CommandFear>(Check);
        this.AddCmd<CommandFixBodyBegin>(Check);
        this.AddCmd<CommandFixBodyLeave>(Check);
        this.AddCmd<CommandWound>(Check);
        this.AddCmd<CommandBeatBack>(Check);
        this.AddCmd<CommandBeatDown>(Check);
        this.AddCmd<CommandBeatFly>(Check);
        this.AddCmd<CommandFloat>(Check);
        this.AddCmd<CommandHook>(Check);
        this.AddCmd<CommandGrab>(Check);
        this.AddCmd<CommandVariation>(Check);
        this.AddCmd<CommandRideBegin>(Check);
        this.AddCmd<CommandRideLeave>(Check);
        this.AddCmd<CommandJump>(Check);
        this.AddCmd<CommandStealthBegin>(Check);
        this.AddCmd<CommandStealthLeave>(Check);
        this.AddCmd<CommandReborn>(Check);
        this.AddCmd<CommandMine>(Check);
        this.AddCmd<CommandInterActive>(Check);
        this.AddCmd<CommandDance>(Check);
        this.AddCmd<CommandRoll>(Check);
    }

    void AddCmd<T>(CmdHandler<T> del) where T : ICommand, new()
    {
        T t = Activator.CreateInstance<T>();
        t.Del = del;
        m_Cmds.Add(t.GetType(), t);
    }

    public T    GetCmd<T>() where T : ICommand
    {
        ICommand iCmd = null;
        m_Cmds.TryGetValue(typeof(T), out iCmd);
        if (iCmd == null)
        {
            GTLog.E(GTLogTag.TAG_ACTOR, string.Format("Not Register ICmd={0}", typeof(T)));
            return null;
        }
        T t = (T)iCmd;
        return t;
    }

    Resp Check(CommandIdle cmd)
    {
        if (m_Actor.IsFSMLayer1() || m_Actor.IsFSMLayer2())
        {
            return Resp.TYPE_NO;
        }
        if (m_Actor.IsCastingSkill())
        {
            return Resp.TYPE_NO;
        }
        m_Actor.DoIdle();
        return Resp.TYPE_YES;
    }

    Resp Check(CommandMove cmd)
    {
        if (m_Actor.CanMove() == false)
        {
            return Resp.TYPE_ACTOR_CANNOT_MOVE;
        }
        switch (cmd.MoveType)
        {
            case EMoveType.MoveForce:
                m_Actor.DoSimpleMove(cmd.Motion);
                break;
            case EMoveType.SeekActor:
                if (m_ActorPathFinding.IsCanReachPosition(cmd.TargetActor.Pos) == false)
                {
                    return Resp.TYPE_ACTOR_CANNOT_MOVETODEST;
                }
                m_Actor.DoNavigateToTarget(cmd.TargetActor.Pos, cmd.OnFinish);
                break;
            case EMoveType.SeekTransform:
                if (m_ActorPathFinding.IsCanReachPosition(cmd.Target.position) == false)
                {
                    return Resp.TYPE_ACTOR_CANNOT_MOVETODEST;
                }
                m_Actor.DoNavigateToTarget(cmd.Target.position, cmd.OnFinish);
                break;
            case EMoveType.SeekPosition:
                if (m_ActorPathFinding.IsCanReachPosition(cmd.DestPosition) == false)
                {
                    return Resp.TYPE_ACTOR_CANNOT_MOVETODEST;
                }
                m_Actor.DoNavigateToDestination(cmd.DestPosition, cmd.OnFinish);
                break;
        }
        return Resp.TYPE_YES;
    }

    Resp Check(CommandJump cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandDead cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandTalk cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandWound cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandBeatBack cmd)
    {
        if (m_Actor.IsDead())
        {
            return Resp.TYPE_ACTOR_DEAD;
        }
        //m_Actor.DoBeatBack(cmd.MaxDis);
        return Resp.TYPE_YES;
    }

    Resp Check(CommandBeatFly cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandBeatDown cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandFloat cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandHook cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandGrab cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandFrost cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandStun cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandParaly cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandSleep cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandBlind cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandFear cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandFixBodyBegin cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandFixBodyLeave cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandRideBegin cmd)
    {
        GTNetworkSend.Instance.TryRideOnMount(m_Actor.GUID);
        return Resp.TYPE_YES;
    }

    Resp Check(CommandRideLeave cmd)
    {
       
        return Resp.TYPE_YES;
    }

    Resp Check(CommandReborn cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandInterActive cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandMine cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandRoll cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandDance cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandStealthBegin cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandStealthLeave cmd)
    {
        return Resp.TYPE_YES;
    }

    Resp Check(CommandVariation cmd)
    {
        return Resp.TYPE_NO;
    }

    public Resp ManualUseSkill(ESkillPos pos)
    {
        ActSkill skill = m_ActorSkill.GetSkill(pos);
        if (skill == null)
        {
            return Resp.TYPE_SKILL_NOTFIND;
        }

        if (skill.IsAutoLockTarget())
        {
            switch (skill.m_HitShipType)
            {
                case EHitShipType.EHST_ALL:
                    break;
                case EHitShipType.EHST_FRIEND:
                    if (m_Actor.Match(EAffect.Ally, m_Actor.Target) == false)
                    {
                        Actor target = m_Actor.GetTargetByPolicy(EAffect.Ally, ESelectTargetPolicy.TYPE_SELECT_BY_LESSDISTANCE);
                        GTWorld.Instance.SetTarget(m_Actor, target);
                    }
                    break;
                case EHitShipType.EHST_ENEMY:
                    if (m_Actor.Match(EAffect.Enem, m_Actor.Target) == false)
                    {
                        Actor target = m_Actor.GetTargetByPolicy(EAffect.Enem, ESelectTargetPolicy.TYPE_SELECT_BY_LESSDISTANCE);
                        GTWorld.Instance.SetTarget(m_Actor, target);
                    }
                    break;
            }
        }
        Resp resp = m_Actor.CanUseSkill(skill);
        switch (resp)
        {
            case Resp.TYPE_SKILL_TOOFAR:
                if (skill.IsAutoNavigateTo())
                {
					GetCmd<CommandMove>().Update( m_Actor.Target.Pos, () => { ManualUseSkill(pos); }).Do();
                }
                else
                {
                    List<Actor> targets = m_Actor.Target == null ? null : new List<Actor>() { m_Actor.Target };
                    GTNetworkSend.Instance.TryCastSkill(skill.ID, m_Actor, targets, m_Actor.Pos + m_Actor.Dir);
                }
                break;
            case Resp.TYPE_YES:
                if (skill.IsAutoRotateTo() && m_Actor.Target != null)
                {
                    m_Actor.FaceTarget(0);
                    List<Actor> targets = m_Actor.Target == null ? null : new List<Actor>() { m_Actor.Target };
                    GTNetworkSend.Instance.TryCastSkill(skill.ID, m_Actor, targets, Vector3.zero);
                }
                break;
        }
        return resp;
    }
}

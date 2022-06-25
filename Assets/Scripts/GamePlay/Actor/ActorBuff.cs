using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using ACT;
using ECT;
using System.Linq;

public class Buff
{
    public Int32 ID { get; private set; }
    public DBuff DB { get; private set; }
    public ulong EffectGuid { get; private set; }

    private Actor m_Actor = null;

    public Buff(int id, Actor target)
    {
        this.ID = id;

        this.DB = ReadCfgBuff.GetDataById(id);

        this.m_Actor = target;

        Transform bindTransform = target.Get<ActorAvator>().GetBindTransform(DB.EffectBind);

        ActorEffect ActEff = target.Get<ActorEffect>();

        this.EffectGuid = ActEff.AddEffect(DB.EffectID, target.CacheTransform, DB.Duration, true, Vector3.zero,Vector3.zero, Vector3.one);
    }

    public void Release()
    {
        if (EffectGuid == 0)
        {
            return;
        }

        ActorEffect ActEff = this.m_Actor.Get<ActorEffect>();
        if(ActEff == null)
        {
            return;
        }

        ActEff.DelEffect(EffectGuid);

        EffectGuid = 0;
    }
}

public class ActorBuff : IActorComponent
{
    private Actor      m_Actor      = null;
    private List<Buff> m_Buffs      = new List<Buff>();

    public void Initial(Actor actor)
    {
        m_Actor = actor;
    }

    public void Execute()
    {

    }

    public void Release()
    {
        DelelteAll();
    }

    public Buff GetBuff(int id)
    {
        for (int i = 0; i < m_Buffs.Count; i++)
        {
            Buff child = m_Buffs[i];
            if (child.ID == id)
            {
                return child;
            }
        }
        return null;
    }

    public bool AddBuff(int id)
    {
        Buff newBuff = new Buff(id, m_Actor);

        m_Buffs.Add(newBuff);

        return true;
    }

    public bool DelBuff(int id)
    {
        for (int i = 0; i < m_Buffs.Count; i++)
        {
            if(m_Buffs[i].ID == id)
            {
                m_Buffs[i].Release();
                m_Buffs.RemoveAt(i);
               return true;
            }
        }
        return false;
    }

    public void DelBuffByType(EBuffType type, int delBuffNum = 0)
    {
        int num = 0;
        for (int i = 0; i < m_Buffs.Count; i++)
        {
            Buff current = m_Buffs[i];
            if (current.DB.Type == (int)type)
            {
                num++;
            }
            if (delBuffNum > 0 && num >= delBuffNum)
            {
                break;
            }
        }
    }
    public void DelelteAll()
    {
        for (int i = 0; i < m_Buffs.Count; i++)
        {
            m_Buffs[i].Release();
        }
        m_Buffs.Clear();
    }

    public List<Buff> Buffs
    {
        get { return m_Buffs; }
    }
}

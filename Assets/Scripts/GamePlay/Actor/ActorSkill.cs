using UnityEngine;
using System.Collections;
using System;
using ACT;
using System.Collections.Generic;
using Protocol;

public class ActorSkill : IActorComponent
{
    private Actor               m_Actor              = null;
    private ActSkill            m_CurrentSkill       = null;
    private List<ActSkill>      m_GeneralSkills      = new List<ActSkill>();
    private List<ActSkill>      m_SpecialSkills      = new List<ActSkill>();
    public void Initial(Actor c)
    {
        this.m_Actor = c;

        for (int i = 0; i < this.m_Actor.Skills.Count; i++)
        {
            SkillItem sItem = this.m_Actor.Skills[i];

            ActSkill skill = BattleSkillData.GetActSkill((int)sItem.SkillID);
            if(skill == null)
            {
                continue;
            }

            ActSkill sCloneSkill = skill.Clone();
            sCloneSkill.Pos = (ESkillPos)sItem.KeyPos;
            sCloneSkill.m_SkillInfo = ReadCfgSkill.GetSkillDataById((int)sItem.SkillID, sItem.Level);
          
            if (sCloneSkill.Pos == ESkillPos.Skill_0)
            {
                m_GeneralSkills.Add(sCloneSkill);
            }
            else
            {
                m_SpecialSkills.Add(sCloneSkill);
            }
            
        }
    }

    public void Execute()
    {
        if (m_CurrentSkill != null)
        {
            m_CurrentSkill.Loop();
            if (m_CurrentSkill.Status == EACTS.SUCCESS)
            {
                m_CurrentSkill.Reset();
                m_CurrentSkill = null;
               
            }
        }
    }

    public void Release()
    {
        StopCurrentSkill();
    }

    public bool PlaySkill(int id, Callback callback)
    {
        StopCurrentSkill();
        ActSkill skill = GetSkill(id);
        if(skill == null)
        {
            Debug.LogError("Error PlaySkill skill: " + id.ToString() + "not exist!");
            return false;
        }
        PlaySkill(skill, callback);
        return true;
    }

    public void PlaySkill(ActSkill skill, Callback callback)
    {
        m_CurrentSkill = skill;
        m_CurrentSkill.Reset();
        m_CurrentSkill.CasterActor = m_Actor;
        m_CurrentSkill.TargetActor = m_Actor.Target;
        m_CurrentSkill.OnComplete = callback;
    }

    public void StopCurrentSkill()
    {
        if (m_CurrentSkill != null)
        {
            m_CurrentSkill.Stop();
            m_CurrentSkill = null;
        }
    }

    public ActSkill GetSkill(int id)
    {
        for (int i = 0; i < m_SpecialSkills.Count; i++)
        {
            if (m_SpecialSkills[i].ID == id)
            {
                return m_SpecialSkills[i];
            }
        }
        for (int i = 0; i < m_GeneralSkills.Count; i++)
        {
            if (m_GeneralSkills[i].ID == id)
            {
                return m_GeneralSkills[i];
            }
        }
        return null;
    }

    public ActSkill GetSkill(ESkillPos pos)
    {
        if (pos == ESkillPos.Skill_0)
        {
            return m_GeneralSkills[0];
        }
        else
        {
            for (int i = 0; i < m_SpecialSkills.Count; i++)
            {
                ActSkill skill = m_SpecialSkills[i];
                if (skill.Pos == pos)
                {
                    return skill;
                }
            }
            return null;
        }
    }

    public ActSkill GetCurrent()
    {
        return m_CurrentSkill;
    }
    public List<ActSkill> SpecialSkills
    {
        get { return m_SpecialSkills; }
    }

    public List<ActSkill> GeneralSkills
    {
        get { return m_GeneralSkills; }
    }
}
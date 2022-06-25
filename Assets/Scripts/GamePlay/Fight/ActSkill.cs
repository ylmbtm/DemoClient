using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Xml;
using Protocol;

namespace ACT
{
    public partial class ActSkill : ActNode
    {
        public int                   ID               = 0;
        public string                Name             = "新技能";
        public float                 CastDistance     = 0;
        public ESkillCastType        CastType         = ESkillCastType.SKILL_SELECT_TYPE_INSTANT;
        public float                 Duration         = 0;
        //public EAnimatorLayerType    LayerType        = EAnimatorLayerType.TYPE_ANIMATOR_LAYER0;
        public List<ActItem>         Items            = new List<ActItem>();
       
        public float             CD { get { return m_SkillInfo.CD; } }
        public EACTS             Status      { get; private set; }
        public bool              FirstUse    { get; private set; }
        public float             StatTime    { get; private set; }
        public float             PastTime    { get { return Time.realtimeSinceStartup - StatTime; } }
        public float             LeftTime    { get { return CD - PastTime; } }
        public ESkillPos         Pos         { get; set; }
        public Actor             CasterActor { get; set; }
        public Actor             TargetActor { get; set; }
        public Callback          OnComplete  { get; set; }
        public DSkill           m_SkillInfo { get;  set; }
        public EHitShipType m_HitShipType { get { return m_SkillInfo.HitShipType; } }
        public ActSkill Clone()
        {
            ActSkill skill = new ActSkill();
            skill.ID = this.ID;
            skill.CastDistance = this.CastDistance;
            skill.CastType = this.CastType;
            //skill.LayerType = this.LayerType;
            skill.FirstUse = true;
            skill.Duration = this.Duration;
            for (int i = 0; i < this.Items.Count; i++)
            {
                ActItem item = this.Items[i].Clone();
                skill.Items.Add(item);
            }

            return skill;
        }

        public ActItem AddChild(Type type)
        {
            ActItem child = (ActItem)System.Activator.CreateInstance(type);
            if(child == null)
            {
                return null;
            }
            Items.Add(child);
            return child;
        }

        public ActItem DelChild(ActItem data)
        {
            Items.Remove(data);
            return null;
        }

        public void Begin()
        {
            this.Status   = EACTS.STARTUP;
            this.FirstUse = false;
            this.StatTime = Time.realtimeSinceStartup;
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Skill  = this;
            }
            this.Status = EACTS.RUNNING;
        }

        public void Loop()
        {
            if (Status == EACTS.INITIAL)
            {
                Begin();
            }

            if (Status == EACTS.RUNNING)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    Items[i].Loop();
                }

                if (PastTime > Duration)
                {
                    Exit();
                }
            }
        }

        public void Stop()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Stop();
            }
            this.Status = EACTS.SUCCESS;
            
            if (OnComplete != null)
            {
                OnComplete();
                OnComplete = null;
            }
        }

        public void Exit()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Exit();
            }
            this.Status = EACTS.SUCCESS;
           
            if (OnComplete != null)
            {
                OnComplete();
                OnComplete = null;
            }
        }

        public void Reset()
        {
            this.Status = EACTS.INITIAL;
        }

        public bool IsCD()
        {
            if (this.FirstUse)
            {
                return false;
            }
            if (this.CD > 0 && (PastTime < this.CD))
            {
                return true;
            }
            return false;
        }

        public bool IsAutoNavigateTo()
        {
            return CastType == ESkillCastType.SKILL_SELECT_TYPE_INSTANT ||
                   CastType == ESkillCastType.SKILL_SELECT_TYPE_TARGET;
        }

        public bool IsAutoRotateTo()
        {
            return CastType == ESkillCastType.SKILL_SELECT_TYPE_INSTANT ||
                   CastType == ESkillCastType.SKILL_SELECT_TYPE_TARGET ||
                   CastType == ESkillCastType.SKILL_SELECT_TYPE_DIRECTION;
        }

        public bool IsAutoLockTarget()
        {
            return CastType == ESkillCastType.SKILL_SELECT_TYPE_INSTANT ||
                   CastType == ESkillCastType.SKILL_SELECT_TYPE_TARGET;
        }

        public bool CanMove()
        {
            return true;// LayerType == EAnimatorLayerType.TYPE_ANIMATOR_LAYER2;
        }

        public bool CanTurn()
        {
            return true;//LayerType == EAnimatorLayerType.TYPE_ANIMATOR_LAYER2;
        }

        public override void Load(XmlElement element)
        {
            base.Load(element);
            XmlElement child = element.FirstChild as XmlElement;
            while (child != null)
            {
                Type type = System.Type.GetType("ACT" + "." + child.Name);
                if(type == null)
                {
                    child = child.NextSibling as XmlElement;
                    continue;
                }

                try
                {
                    ActItem item = AddChild(type);
                    if (item == null)
                    {
                        child = child.NextSibling as XmlElement;
                        continue;
                    }
                    item.Load(child);
                    child = child.NextSibling as XmlElement;
                }
                catch (Exception ex)
                {
                    child = child.NextSibling as XmlElement;
                }
            }
        }

        public override void Save(XmlDocument doc, XmlElement element)
        {
            base.Save(doc, element);
            for (int i = 0; i < Items.Count; i++)
            {
                ActItem it = Items[i];
                XmlElement child = doc.CreateElement(it.GetType().Name);
                element.AppendChild(child);
                it.Save(doc, child);
            }
        }
    }
}
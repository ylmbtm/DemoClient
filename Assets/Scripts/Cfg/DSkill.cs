using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using ACT;
using System.Collections.Generic;
using Protocol;

public class DSkill : DObj<int>
{
    public int                 Id;
    public int                 Level;
    public string              Name          = string.Empty;
    public string              Icon          = string.Empty;
    public string              Desc          = string.Empty;
    public float               SkillDistance = 0;
    public float               SkillRadius   = 0;
    public float               SkillAngles   = 0;
    public float               CD            = 0;
    public ESkillCostType      CostType      = ESkillCostType.NO;
    public EHitShipType        HitShipType = EHitShipType.EHST_ENEMY;
    public Int32               CostItemID    = 0;
    public Int32               CostNum       = 0;
    public float                Duration = 0;

    public override int GetKey()
    {
        return Level << 20 | Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id              = element.GetInt32("Id");
        this.Level           = element.GetInt32("Level");
        this.Name            = element.GetString("Name");
        this.Icon            = element.GetString("Icon");
        this.Desc            = element.GetString("Desc");
        this.CD              = element.GetFloat("CountDown")/1000.0f;
        this.CostType        = (ESkillCostType)element.GetInt32("CostType");
        this.HitShipType     = (EHitShipType)element.GetInt32("HitShipType");
        this.CostItemID      = element.GetInt32("CostItemID");
        this.CostNum         = element.GetInt32("CostNum");
    }
}

public class ReadCfgSkill : DReadBase<int, DSkill>
{
    public static DSkill GetSkillDataById(int skillid, int level)
    {
        int nkey = level << 20 | skillid;
        DSkill v = GetDataById(nkey);
        return v;
    }
}
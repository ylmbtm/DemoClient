using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Collections.Generic;

public class DBuff : DObj<int>
{
    public Int32            Id;
    public string           Name = string.Empty;
    public Int32            Lv;
    public Int32            Type;
    public string           Icon  = string.Empty;
    public Int32            TimeType;
    public Int32            DestroyType;
    public Int32            OverlayType;
    public Int32            OverlayNum;
    public float            Duration;
    public bool             CanDispel;
    public Int32            EffectID;
    public EBind            EffectBind;
    public string           Sound           = string.Empty;
    public string           Desc            = string.Empty;
    public List<int>        Items           = new List<int>();
    public List<int>        EnterBehvaiours = new List<int>();
    public List<int>        LeaveBehaviours = new List<int>();

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id              = element.GetInt32("Id");
        this.Name            = element.GetString("Name");
        this.Lv              = element.GetInt32("Lv");
        this.Type            = element.GetInt32("Type");
        this.Icon            = element.GetString("Icon");
        this.DestroyType     = element.GetInt32("DestroyType");
        this.OverlayType     = element.GetInt32("OverlayType");
        this.OverlayNum      = element.GetInt32("OverlayNum");
        this.Duration        = element.GetInt32("Duration");
        this.CanDispel       = element.GetBool("CanDispel");
        this.EffectID        = element.GetInt32("EffectID");
        this.EffectBind      = (EBind)element.GetInt32("EffectBind");
        this.Sound           = element.GetString("Sound");
        this.Desc            = element.GetString("Desc");
        this.Items           = element.GetListForInt("Items");
        this.EnterBehvaiours = element.GetListForInt("EnterBehvaiours");
        this.LeaveBehaviours = element.GetListForInt("LeaveBehaviours");
    }
}

public class ReadCfgBuff: DReadBase<int, DBuff>
{

}
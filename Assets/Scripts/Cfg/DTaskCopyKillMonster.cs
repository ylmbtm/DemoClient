using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DTaskCopyKillMonster : DObj<int>
{
    public int Id;
    public int CopyID;
    public int MonsterID;
    public int MonsterCount;
    public int TVSt;
    public int TVEd;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id           = element.GetInt32("Id");
        this.CopyID       = element.GetInt32("CopyID");
        this.MonsterID    = element.GetInt32("MonsterID");
        this.MonsterCount = element.GetInt32("MonsterCount");
        this.TVSt         = element.GetInt32("TVSt");
        this.TVEd         = element.GetInt32("TVEd");
    }
}

public class ReadCfgTaskCopyKillMonster : DReadBase<int, DTaskCopyKillMonster>
{

}
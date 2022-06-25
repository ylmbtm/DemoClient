using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DTaskHunter : DObj<int>
{
    public int Id;
    public int MonsterID;
    public int MonsterCount;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id           = element.GetInt32("Id");
        this.MonsterID    = element.GetInt32("MonsterID");
        this.MonsterCount = element.GetInt32("MonsterCount");
    }
}

public class ReadCfgTaskHunter : DReadBase<int, DTaskHunter>
{

}
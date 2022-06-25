using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DTaskCollect : DObj<int>
{
    public int Id;
    public int MineID;
    public int MineCount;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    { 
        this.Id        = element.GetInt32("Id");
        this.MineID    = element.GetInt32("MineID");
        this.MineCount = element.GetInt32("MineCount");
    }
}

public class ReadCfgTaskCollect : DReadBase<int, DTaskCollect>
{

}
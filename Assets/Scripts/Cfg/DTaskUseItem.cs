using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DTaskUseItem : DObj<int>
{
    public int Id;
    public int ItemID;
    public int Times;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id     = element.GetInt32("Id");
        this.ItemID = element.GetInt32("ItemID");
        this.Times  = element.GetInt32("Times");
    }
}

public class ReadCfgTaskUseItem : DReadBase<int, DTaskUseItem>
{

}
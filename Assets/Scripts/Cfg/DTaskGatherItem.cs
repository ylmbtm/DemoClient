using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DTaskGatherItem : DObj<int>
{
    public int  Id;
    public int  ItemID;
    public int  ItemCount;
    public bool IsDelAbandon;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id           = element.GetInt32("Id");
        this.ItemID       = element.GetInt32("ItemID");
        this.ItemCount    = element.GetInt32("ItemCount");
        this.IsDelAbandon = element.GetBool("IsDelAbandon");
    }
}

public class ReadCfgTaskGatherItem : DReadBase<int, DTaskGatherItem>
{

}
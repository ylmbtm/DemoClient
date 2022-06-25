using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DTaskDelivery : DObj<int>
{
    public int  Id;
    public int  ItemID;
    public int  ItemCount;
    public bool IsDelItem;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id           = element.GetInt32("Id");
        this.ItemID       = element.GetInt32("ItemID");
        this.ItemCount    = element.GetInt32("ItemCount");
        this.IsDelItem    = element.GetBool("IsDelItem");
    }
}

public class ReadCfgTaskDelivery : DReadBase<int, DTaskDelivery>
{

}
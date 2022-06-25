using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DTaskLootItem : DObj<int>
{
    public Int32 Id;
    public Int32 ItemID;
    public Int32 ItemCount;
    public float ItemDropRate;
    public Int32 MonsterID;
    public bool  IsDelItem;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id           = element.GetInt32("Id");
        this.ItemID       = element.GetInt32("ItemID");
        this.ItemCount    = element.GetInt32("ItemCount");
        this.ItemDropRate = element.GetInt32("ItemDropRate");
        this.MonsterID    = element.GetInt32("MonsterID");
        this.IsDelItem    = element.GetBool("IsDelItem");
    }
}

public class ReadCfgTaskLootItem : DReadBase<int, DTaskLootItem>
{

}
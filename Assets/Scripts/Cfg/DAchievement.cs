using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DAchievement : DObj<int>
{
    public Int32  Id;
    public string Desc;
    public string Content;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id      = element.GetInt32("Id");
        this.Desc    = element.GetString("Desc");
        this.Content = element.GetString("Content");
    }
}

public class ReadCfgAchievement : DReadBase<int, DAchievement>
{

}
using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DBuffItem : DObj<int>
{
    public Int32    Id;
    public string   TypeName = string.Empty;
    public string[] Params;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id       = element.GetInt32("Id");
        this.TypeName = element.GetString("TypeName");
        this.Params   = element.GetString("Params").Split('~');
    }
}

public class ReadCfgBuffItem : DReadBase<int, DBuffItem>
{

}
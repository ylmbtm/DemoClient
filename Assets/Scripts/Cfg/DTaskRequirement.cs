using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DTaskRequirement : DObj<int>
{
    public int      Id;
    public string   Type;
    public string[] Params;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id     = element.GetInt32("Id");
        this.Type   = element.GetString("Type");
        this.Params = element.GetString("Params").Split(',');
    }
}

public class ReadCfgTaskRequirement : DReadBase<int, DTaskRequirement>
{

}
using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DBuffBehaviour : DObj<int>
{
    public Int32    Id;
    public Int32    Type;
    public string[] Params;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id     = element.GetInt32("Id");
        this.Type   = element.GetInt32("Type");
        this.Params = element.GetString("Params").Split('~');
    }
}

public class ReadCfgBuffBehaviour : DReadBase<int, DBuffBehaviour>
{

}
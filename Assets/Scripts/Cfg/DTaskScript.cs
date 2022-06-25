using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DTaskScript : DObj<int>
{
    public int      Id;
    public Int32    ScriptType;
    public string[] ScriptArgs;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id         = element.GetInt32("Id");
        this.ScriptType = element.GetInt32("ScriptType");
        this.ScriptArgs = element.GetString("ScriptArgs").Split(',');
    }
}

public class ReadCfgTaskScript : DReadBase<int, DTaskScript>
{

}
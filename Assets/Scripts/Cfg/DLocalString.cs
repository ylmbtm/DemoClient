using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DLocalString : DObj<uint>
{
    public uint Id;
    public string Value;

    public override uint GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id    = element.GetUInt32("Id");
        this.Value = element.GetString("Language_0");
    }
}

public class ReadCfgLocalString : DReadBase<uint, DLocalString>
{

}
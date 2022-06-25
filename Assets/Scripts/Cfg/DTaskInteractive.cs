using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DTaskInteractive : DObj<int>
{
    public int Id;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id = element.GetInt32("Id");
    }
}

public class ReadCfgTaskInteractive : DReadBase<int, DTaskInteractive>
{

}
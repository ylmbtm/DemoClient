using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Collections.Generic;

public class DTaskSeries : DObj<int>
{
    public int       Id;
    public List<int> Tasks = new List<int>();

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id       = element.GetInt32("Id");
        this.Tasks    = element.GetListForInt("Tasks");
    }
}

public class ReadCfgTaskSeries : DReadBase<int, DTaskSeries>
{

}
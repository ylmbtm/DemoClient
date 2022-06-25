using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DTaskChapter : DObj<int>
{
    public Int32  Id;
    public Int32  Section;
    public string Name = string.Empty;
    public string Desc = string.Empty;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id      = element.GetInt32("Id");
        this.Section = element.GetInt32("Section");
        this.Name    = element.GetString("Name");
        this.Desc    = element.GetString("Desc");
    }
}

public class ReadCfgTaskChapter : DReadBase<int, DTaskChapter>
{

}
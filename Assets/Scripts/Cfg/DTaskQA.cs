using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DTaskQA : DObj<int>
{
    public int    Id;
    public string Question;
    public int    CorrentAnswer;
    public string Option1;
    public string Option2;
    public string Option3;
    public string Option4;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id            = element.GetInt32("Id");
        this.Question      = element.GetString("Question");
        this.CorrentAnswer = element.GetInt32("CorrentAnswer");
        this.Option1       = element.GetString("Option1");
        this.Option2       = element.GetString("Option2");
        this.Option3       = element.GetString("Option3");
        this.Option4       = element.GetString("Option4");
    }
}

public class ReadCfgTaskQA : DReadBase<int, DTaskQA>
{

}
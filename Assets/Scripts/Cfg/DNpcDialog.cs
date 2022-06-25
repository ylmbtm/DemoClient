using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Collections.Generic;

public class DNpcDialog : DObj<int>
{
    public Int32       Id;
    public Int32       Type;
    public string      Dialog;
    public string      OptContent;
    public string      Opt1Text;
    public string      Opt2Text;
    public string      Opt3Text;
    public List<Int32> Tasks = new List<int>();

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id           = element.GetInt32("Id");
        this.Type         = element.GetInt32("Type");
        this.Dialog       = element.GetString("Dialog");
        this.OptContent   = element.GetString("OptContent");
        this.Opt1Text     = element.GetString("Opt1Text");
        this.Opt2Text     = element.GetString("Opt2Text");
        this.Opt3Text     = element.GetString("Opt3Text");
        this.Tasks        = element.GetListForInt("Tasks");
    }
}

public class ReadCfgNpcDialog : DReadBase<int, DNpcDialog>
{

}
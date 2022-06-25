using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DNpcBubble : DObj<int>
{
    public Int32  Id;
    public string Bubble1;
    public string Bubble2;
    public string Bubble3;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id      = element.GetInt32("Id");
        this.Bubble1 = element.GetString("Bubble1");
        this.Bubble2 = element.GetString("Bubble2");
        this.Bubble3 = element.GetString("Bubble3");
    }
}

public class ReadCfgNpcBubble : DReadBase<int, DNpcBubble>
{

}
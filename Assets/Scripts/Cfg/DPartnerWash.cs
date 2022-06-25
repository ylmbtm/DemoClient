using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Xml;

public class DPartnerWash : DObj<int>
{
    public int   Level;

    public override int GetKey()
    {
        return Level;
    }

    public override void Read(XmlElement element)
    {
        this.Level = element.GetInt32("Level");
    }
}

public class ReadCfgPartnerWash : DReadBase<int, DPartnerWash>
{

}
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Xml;

public class DPetLevel : DObj<int>
{
    public int    Id;
    public int    Quality;
    public int    Level;
    public int    Exp;
    public int    Ratio;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id      = element.GetInt32("Id");
        this.Quality = element.GetInt32("Quality");
        this.Level   = element.GetInt32("Level");
        this.Exp     = element.GetInt32("Exp");
        this.Ratio   = element.GetInt32("Ratio");
    }
}

public class ReadCfgPetLevel : DReadBase<int, DPetLevel>
{

}
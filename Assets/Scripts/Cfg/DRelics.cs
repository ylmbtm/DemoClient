using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Xml;

public class DRelics : DObj<int>
{
    public int            Id;
    public string         Name;
    public string         Icon;
    public string         Desc;
    public string         Model;
    public int            ActiveEffectID;
    public int            DitiveEffectID;
    public float          X;
    public float          Y;
    public float          Z;
    public float          Scale;
    public int[]          LevelExps          = new int[5];
    public int[]          ArtificeCostIDs    = new int[3];
    public int[]          PropertyIDs        = new int[3];
    public int[]          PropertyNums       = new int[3];

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id             = element.GetInt32("Id");
        this.Name           = element.GetString("Name");
        this.Icon           = element.GetString("Icon");
        this.Model          = element.GetString("Model");
        this.ActiveEffectID = element.GetInt32("ActiveEffectID");
        this.DitiveEffectID = element.GetInt32("DitiveEffectID");
        this.Desc           = element.GetString("Desc");
        this.X              = element.GetFloat("X");
        this.Y              = element.GetFloat("Y");
        this.Z              = element.GetFloat("Z");
        this.Scale          = element.GetFloat("Scale");

        for (int i = 1; i <= 5; i++)
        {
            int exp = element.GetInt32("LevelExp" + i);
            this.LevelExps[i - 1] = exp;
        }
        for (int i = 1; i <= 3; i++)
        {
            int id = element.GetInt32("ArtificeCostID" + i);
            this.ArtificeCostIDs[i - 1] = id;
        }
        for (int i = 1; i <= 3; i++)
        {
            int p   = element.GetInt32("PropertyID" + i);
            int num = element.GetInt32("PropertyNum" + i);
            this.PropertyIDs [i - 1] = p;
            this.PropertyNums[i - 1] = num;
        }
    }
}

public class ReadCfgRelics : DReadBase<int, DRelics>
{

}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;

public class DEquipAdvance : DObj<int>
{
    public int                                Id;
    public string                             Name;
    public int                                Quality;
    public List<KeyValuePair<int, int>>       Attrs = new List<KeyValuePair<int, int>>();

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id        = element.GetInt32("Id");
        this.Name      = element.GetString("Name");
        this.Quality   = element.GetInt32("Quality");
        for (int i = 1; i <= 8; i++)
        {
            int key    = element.GetInt32("PropertyId" + i);
            int value  = element.GetInt32("PropertyNum" + i);
            KeyValuePair<int, int> e = new KeyValuePair<int, int>(key, value);
            this.Attrs.Add(e);
        }
    }
}

public class ReadCfgEquipAdvance : DReadBase<int, DEquipAdvance>
{

}
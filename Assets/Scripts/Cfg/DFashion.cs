using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Xml;

public class DFashion : DObj<int>
{
    public int                                Id;
    public List<KeyValuePair<int, int>>     Propertys = new List<KeyValuePair<int, int>>();

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id = element.GetInt32("Id");
        for (int i = 1; i <= 2; i++)
        {
            int e = element.GetInt32("PropertyId" + i);
            int v = element.GetInt32("PropertyNum" + i);
            KeyValuePair<int, int> fp = new KeyValuePair<int, int>(e, v);
            this.Propertys.Add(fp);
        }
    }
}

public class ReadCfgFashion : DReadBase<int, DFashion>
{

}

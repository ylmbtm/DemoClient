using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;

public class DEquipSuit : DObj<int>
{
    public int                              Id;
    public string                           Name      = string.Empty;
    public List<Dictionary<int, int>>       SuitAttrs = new List<Dictionary<int, int>>();

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id   = element.GetInt32("Id");
        this.Name = element.GetString("Name");
        for (int i = 1; i <= 3; i++)
        {
            string[] stArray = element.GetString("Suit" + i).Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            string[] idArray = element.GetString("SuitPropertyId" + i).Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<int, int> d = new Dictionary<int, int>();
            for (int j = 0; j < stArray.Length; j++)
            {
                int e = idArray[j].ToInt32();
                int v = stArray[j].ToInt32();
                if (!d.ContainsKey(e))
                {
                    d.Add(e, v);
                }
            }
            this.SuitAttrs.Add(d);
        }
    }
}

public class ReadCfgEquipSuit : DReadBase<int, DEquipSuit>
{

}
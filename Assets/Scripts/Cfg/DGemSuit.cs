using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

public class DGemSuit : DObj<int>
{
    public int                              Id;
    public string                           Name;
    public List<Dictionary<int, int>>       SuitAttrs = new List<Dictionary<int, int>>();
    public string                           SuitDesc;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id       = element.GetInt32("Id");
        this.Name     = element.GetString("Name");
        this.SuitDesc = element.GetString("SuitDesc");
        for (int i = 1; i <= 3; i++)
        {
            string[] suitArray = element.GetString("Suit" + i).Split('|');
            Dictionary<int, int> attrs = new Dictionary<int, int>();
            attrs.Add(1, suitArray[0].ToInt32());
            attrs.Add(2, suitArray[1].ToInt32());
            this.SuitAttrs.Add(attrs);
        }
    }
}


public class ReadCfgGemSuit : DReadBase<int, DGemSuit>
{

}
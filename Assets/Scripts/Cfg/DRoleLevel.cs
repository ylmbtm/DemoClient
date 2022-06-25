using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Protocol;

public class DRoleLevel : DObj<int>
{
    public int                        Level;
    public int                        RequireExp;
    public int                        NextLevel;
    public List<int>                  Attrs = new List<int>();

    public override int GetKey()
    {
        return Level;
    }

    public override void Read(XmlElement element)
    {
        this.Level      = element.GetInt32("Level");
        this.RequireExp = element.GetInt32("RequireExp");
        this.NextLevel  = element.GetInt32("NextLevel");
        for (int i = 0; i < Enum.GetNames(typeof(EAttrID)).Length; i++)
        {
            int v = element.GetInt32("P" + i.ToString());
            this.Attrs.Add(v);
        }
    }
}

public class ReadCfgRoleLevel : DReadBase<int, DRoleLevel>
{

}

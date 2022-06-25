using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

public class DProperty : DObj<int>
{
    public int         Id;
    public string      Name;
    public int         Factor;
    public bool        IsPercent;
    public string      Desc;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id        = element.GetInt32("Id");
        this.Name      = element.GetString("Name");
        this.Factor    = element.GetInt32("Factor");
        this.Desc      = element.GetString("Desc");
        this.IsPercent = element.GetBool("IsPercent");
    }
}

public class ReadCfgProperty : DReadBase<int, DProperty>
{

}
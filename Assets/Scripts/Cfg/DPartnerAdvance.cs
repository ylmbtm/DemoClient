using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;

public class DPartnerAdvance : DObj<int>
{
    public int                        Id;
    public int                        Partner;
    public int                        Advance;
    public int                        CostSoulNum;
    public int                        MainTarget;
    public int                        ViceTarget;
    public string                     Desc1;
    public string                     Desc2;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id          = element.GetInt32("Id");
        this.Partner     = element.GetInt32("Partner");
        this.Advance     = element.GetInt32("Advance");
        this.CostSoulNum = element.GetInt32("CostSoulNum");
        this.MainTarget  = element.GetInt32("MainTarget");
        this.ViceTarget  = element.GetInt32("ViceTarget");
        this.Desc1       = element.GetString("Desc1");
        this.Desc2       = element.GetString("Desc2");
    }
}

public class ReadCfgPartnerAdvance : DReadBase<int, DPartnerAdvance>
{

}
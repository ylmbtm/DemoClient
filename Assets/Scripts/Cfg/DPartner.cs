using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;

public class DPartner : DObj<int>
{
    public int    Id;
    public int    ActorId;
    public int    SoulItemID;
    public int[]  Fetters = new int[6];
    public int[]  Skills  = new int[4];
    public bool   Show    = false;
	public string Name    = "";

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id         = element.GetInt32("Id");
        this.ActorId = element.GetInt32("ActorId");
        this.SoulItemID = element.GetInt32("SoulItemID");
		this.Name= element.GetString("Name");
        for (int i = 1; i <= 6; i++)
        {
            this.Fetters[i - 1] = element.GetInt32("Fetter" + i);
        }
        for (int i = 1; i <= 4; i++)
        {
            this.Skills[i - 1]  = element.GetInt32("Skill" + i);
        }
        this.Show       = element.GetBool("Show");
    }
}

public class ReadCfgPartner : DReadBase<int, DPartner>
{

}
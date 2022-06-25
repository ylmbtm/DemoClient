using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DPet : DObj<int>
{
    public int Id;
    public int ActorId;
    public string Name = "";
    public float X;
    public float Y;
    public float Z;
    public float Scale;
    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id = element.GetInt32("Id");
        this.ActorId = element.GetInt32("ActorId");
        this.Name = element.GetString("Name");
        this.X = element.GetFloat("X");
        this.Y = element.GetFloat("Y");
        this.Z = element.GetFloat("Z");
        this.Scale = element.GetFloat("Scale");
    }
}

public class ReadCfgPet : DReadBase<int, DPet>
{

}
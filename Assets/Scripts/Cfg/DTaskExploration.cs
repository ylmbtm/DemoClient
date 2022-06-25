using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DTaskExploration : DObj<int>
{
    public Int32 Id;
    public Int32 TarSceneID;
    public Int32 TarMutiPointID;
    public Int32 TarInSky;
    public float TarPosRadius;
    public Int32 TarTVID;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id             = element.GetInt32("Id");
        this.TarSceneID     = element.GetInt32("TarSceneID");
        this.TarMutiPointID = element.GetInt32("TarMutiPointID");
        this.TarInSky       = element.GetInt32("TarInSky");
        this.TarPosRadius   = element.GetFloat("TarPosRadius");
        this.TarTVID        = element.GetInt32("TarTVID");
    } 
}

public class ReadCfgTaskExploration : DReadBase<int, DTaskExploration>
{

}

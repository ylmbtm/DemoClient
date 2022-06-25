using UnityEngine;
using System.Collections;
using System;
using System.Xml;

namespace CFG
{
    public class MapActTriggerPlayerEffect : MapAction
    {
        public Int32   EffectID;
        public float   EffectLifeTime;
        public Vector3 Pos;
        public Vector3 EulerAngles;

        public override void Read(XmlElement os)
        {
            this.ID             = os.GetInt32("ID");
            this.EffectID       = os.GetInt32("EffectID");
            this.EffectLifeTime = os.GetFloat("EffectLifeTime");
            this.Pos            = os.GetVector3("Pos");
            this.EulerAngles    = os.GetVector3("EulerAngles");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",             ID);
            DCFG.Write(doc, os, "EffectID",       EffectID);
            DCFG.Write(doc, os, "EffectLifeTime", EffectLifeTime);
            DCFG.Write(doc, os, "Pos",            Pos);
            DCFG.Write(doc, os, "EulerAngles",    EulerAngles);
        }
    }
}
using UnityEngine;
using System.Collections;
using System;
using System.Xml;

namespace CFG
{
    public class MapActCameraEffect : MapAction
    {
        public Int32 CameraEffectID = 1;

        public override void Read(XmlElement os)
        {
            this.ID             = os.GetInt32("ID");
            this.CameraEffectID = os.GetInt32("CameraEffectID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",             ID);
            DCFG.Write(doc, os, "CameraEffectID", CameraEffectID);
        }
    }
}
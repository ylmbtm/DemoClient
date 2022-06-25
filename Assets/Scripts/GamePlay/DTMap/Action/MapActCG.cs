using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace CFG
{
    public class MapActCG : MapAction
    {
        public Int32 VideoID = 1;

        public override void Read(XmlElement os)
        {
            this.ID         = os.GetInt32("ID");
            this.VideoID    = os.GetInt32("VideoID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",         ID);
            DCFG.Write(doc, os, "VideoID",    VideoID);
        }
    }
}
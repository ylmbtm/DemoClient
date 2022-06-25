using UnityEngine;
using System.Collections;
using System.Xml;

namespace CFG
{
    public class MapActDestroyBarrier : MapAction
    {
        public int BarrierID;

        public override void Read(XmlElement os)
        {
            this.ID        = os.GetInt32("ID");
            this.BarrierID = os.GetInt32("BarrierID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",        ID);
            DCFG.Write(doc, os, "BarrierID", BarrierID);
        }
    }
}
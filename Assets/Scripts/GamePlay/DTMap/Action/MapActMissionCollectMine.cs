using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace CFG
{
    public class MapActMissionCollectMine : MapAction
    {
        public List<EItem> List = new List<EItem>();

        public override void Read(XmlElement os)
        {
            this.ID   = os.GetInt32("ID");
            this.List = os.GetListForItem("List");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",   ID);
            DCFG.Write(doc, os, "List", List);
        }
    }
}
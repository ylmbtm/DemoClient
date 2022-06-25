using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace CFG
{
    public class MapActWeight : MapActComposite
    {
        public List<int>   Actions = new List<int>();
        public List<float> Weights = new List<float>();

        public override void Read(XmlElement os)
        {
            this.ID      = os.GetInt32("ID");
            this.Actions = os.GetListForInt("Actions");
            this.Weights = os.GetListForFloat("Weights");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",      this.ID);
            DCFG.Write(doc, os, "Actions", this.Actions);
            DCFG.Write(doc, os, "Weights", this.Weights);
        }
    }
}
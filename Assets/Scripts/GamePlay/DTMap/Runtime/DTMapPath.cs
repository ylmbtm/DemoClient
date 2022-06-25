using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace CFG
{
    public class DTMapPath : DTElement
    {
        public int               Type;
        public bool              PositionVary = true;
        public bool              RotationVary = true;
        public List<DTPathNode> PathNodes    = new List<DTPathNode>();

        public override void Read(XmlElement os)
        {
            this.ID           = os.GetInt32("ID");
            this.Type         = os.GetInt32("Type");
            this.PositionVary = os.GetBool("PositionVary");
            this.RotationVary = os.GetBool("RotationVary");
            this.PathNodes    = ReadListFromChildAttribute<DTPathNode>(os, "PathNodes");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",           this.ID);
            DCFG.Write(doc, os, "Type",         this.Type);
            DCFG.Write(doc, os, "PositionVary", this.PositionVary);
            DCFG.Write(doc, os, "RotationVary", this.RotationVary);
            DCFG.Write(doc, os, "PathNodes",    this.PathNodes);
        }
    }
}


using UnityEngine;
using System.Collections;
using System.Xml;

namespace CFG
{
    public class MapActTriggerSkill : MapAction
    {
        public int SkillID;

        public override void Read(XmlElement os)
        {
            this.ID      = os.GetInt32("ID");
            this.SkillID = os.GetInt32("SkillID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",      ID);
            DCFG.Write(doc, os, "SkillID", SkillID);
        }
    }
}
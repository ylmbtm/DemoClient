using UnityEngine;
using System.Collections;
using System.Xml;

namespace CFG
{
    public class DTMonster : DCFG
    {
        public int      MonsterID;
        public int      MonsterType;
        public int      MonsterAI;
        public int      Camp;
        public int      DropID;
        public Vector3  Pos;
        public float    Face;

        public override void Read(XmlElement os)
        {
            this.MonsterID   = os.GetInt32("MonsterID");
            this.MonsterType = os.GetInt32("MonsterType");
            this.MonsterAI   = os.GetInt32("MonsterAI");
            this.DropID      = os.GetInt32("DropID");
            this.Pos         = os.GetVector3("Pos");
            this.Face        = os.GetFloat("Face");
            this.Camp        = os.GetInt32("Camp");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "MonsterID",   this.MonsterID);
            DCFG.Write(doc, os, "MonsterType", this.MonsterType);
            DCFG.Write(doc, os, "MonsterAI",   this.MonsterAI);
            DCFG.Write(doc, os, "DropID",      this.DropID);
            DCFG.Write(doc, os, "Pos",         this.Pos);
            DCFG.Write(doc, os, "Face",        this.Face);
            DCFG.Write(doc, os, "Camp",        this.Camp);
        }
    }
}
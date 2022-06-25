using UnityEngine;
using System.Collections;
using System.Xml;

namespace CON
{
    public class CheckOpenUI : CheckBase
    {
        public EWindowID ID;

        public override bool Check()
        {
            GTWindow w = GTWindowManager.Instance.GetWindow(ID);
            return w.IsVisable();
        }

        public override void Read(XmlElement os)
        {
            this.ID = os.GetEnum<EWindowID>("ID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID", ID.ToString());
        }
    }
}
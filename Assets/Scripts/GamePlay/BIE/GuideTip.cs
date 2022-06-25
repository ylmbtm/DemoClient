using UnityEngine;
using System.Collections;
using System;
using System.Xml;

namespace BIE
{
    public class GuideTip : GuideEffect
    {
        public string         TipSound    = string.Empty;
        public string         TipText     = string.Empty;
        public Vector2        TipPosition = Vector2.zero;
        public EGuideGirlPos  TipGirlPos  = EGuideGirlPos.TYPE_NONE;

        public override void Enter()
        {
            UIGuide window = (UIGuide)GTWindowManager.Instance.OpenWindow(EWindowID.UIGuide);
            window.ShowGuideTipEffect(this);
        }

        public override void Execute()
        {

        }

        public override void Finish()
        {

        }

        public override void Read(XmlElement os)
        {
            this.TipSound    = os.GetString("TipSound");
            this.TipPosition = os.GetVector3("TipPosition");
            this.TipText     = os.GetString("TipText");
            this.TipGirlPos  = (EGuideGirlPos)os.GetInt32("TipGirlPos");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "TipSound",        TipSound);
            DCFG.Write(doc, os, "TipPosition",     TipPosition);
            DCFG.Write(doc, os, "TipText",         TipText);
            DCFG.Write(doc, os, "TipGirlPos", (int)TipGirlPos);
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Xml;
using System;

namespace BIE
{
    public class GuideLock : GuideEffect
    {
        public string                LockKey       = "BtnName";
        public EGuideUIOperationType OperationType = EGuideUIOperationType.TYPE_CLICK;
        public EGuideRowType         RowType       = EGuideRowType.TYPE_NONE;
        public EGuideBoardType       BoardType     = EGuideBoardType.TYPE_NONE;
        public Vector2               BoardSize     = new Vector2(100, 100);

        public override void Enter()
        {
            UIGuide window = (UIGuide)GTWindowManager.Instance.OpenWindow(EWindowID.UIGuide);
            window.ShowGuideLockEffect(this);
        }

        public override void Execute()
        {

        }

        public override void Finish()
        {

        }

        public override void Read(XmlElement os)
        {
            this.LockKey         = os.GetString("LockKey");
            this.OperationType   = (EGuideUIOperationType)os.GetInt32("OperationType");
            this.RowType         = (EGuideRowType)os.GetInt32("RowType");
            this.BoardType       = (EGuideBoardType)os.GetInt32("BoardType");
            this.BoardSize       = os.GetVector2("BoardSize");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "LockKey",            LockKey);
            DCFG.Write(doc, os, "OperationType", (int)OperationType);
            DCFG.Write(doc, os, "RowType",       (int)RowType);
            DCFG.Write(doc, os, "BoardType",     (int)BoardType);
            DCFG.Write(doc, os, "BoardSize",          BoardSize);
        }
    }
}
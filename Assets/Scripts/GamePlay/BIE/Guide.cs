using System;
using System.Collections.Generic;
using System.Xml;
using CON;

namespace BIE
{
    public class Guide : DCFG
    {
        public Int32             Id                 = 0;
        public string            Comment            = string.Empty;
        public bool              IsLocked           = true;
        public bool              IsPause            = false;
        public bool              IsCanSkip          = false;
        public bool              IsSavePoint        = false;
        public bool              IsSaveOnAppear     = false;
        public List<CheckBase>   TriggerConditions  = new List<CheckBase>();
        public List<CheckBase>   CompleteConditions = new List<CheckBase>();
        public List<GuideEffect> Effects            = new List<GuideEffect>();

        public EGuideState       State { get; private set; }

        public bool Check()
        {
            for (int i = 0; i < TriggerConditions.Count; i++)
            {
                if (TriggerConditions[i].Check() == false)
                {
                    return false;
                }
            }
            return true;
        }

        public void Start()
        {
            State = EGuideState.TYPE_ENTER;
        }

        public void Enter()
        {
            this.State = EGuideState.TYPE_EXECUTE;
            UIGuide window = (UIGuide)GTWindowManager.Instance.OpenWindow(EWindowID.UIGuide);
            window.ShowGuideBase(this);
            for (int i = 0; i < Effects.Count; i++)
            {
                Effects[i].Enter();
            }
        }

        public void Execute()
        {
            for (int i = 0; i < Effects.Count; i++)
            {
                Effects[i].Execute();
            }
        }

        public void Finish()
        {
            for (int i = 0; i < Effects.Count; i++)
            {
                Effects[i].Finish();
            }
            GTWindowManager.Instance.HideWindow(EWindowID.UIGuide);
            this.State = EGuideState.TYPE_FINISH;
        }

        public void Reset()
        {
            for (int i = 0; i < Effects.Count; i++)
            {
                Effects[i].Finish();
            }
            this.State = EGuideState.TYPE_NONE;
        }

        public override void Read(XmlElement os)
        {
            this.Id             = os.GetInt16("Id");
            this.Comment        = os.GetString("Comment");
            this.IsLocked       = os.GetBool("IsLocked");
            this.IsPause        = os.GetBool("IsPause");
            this.IsCanSkip      = os.GetBool("IsCanSkip");
            this.IsSavePoint    = os.GetBool("IsSavePoint");
            this.IsSaveOnAppear = os.GetBool("IsSaveOnAppear");
            foreach (var current in GetChilds(os))
            {
                switch (current.Name)
                {
                    case "TriggerConditions":
                        this.TriggerConditions  = ReadList<CheckBase>(current);
                        break;
                    case "CompleteConditions":
                        this.CompleteConditions = ReadList<CheckBase>(current);
                        break;
                    case "Effects":
                        this.Effects            = ReadList<GuideEffect>(current);
                        break;
                }
            }
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "Id",                 Id);
            DCFG.Write(doc, os, "Comment",            Comment);
            DCFG.Write(doc, os, "IsLocked",           IsLocked);
            DCFG.Write(doc, os, "IsPause",            IsPause);
            DCFG.Write(doc, os, "IsCanSkip",          IsCanSkip);
            DCFG.Write(doc, os, "IsSavePoint",        IsSavePoint);
            DCFG.Write(doc, os, "TriggerConditions",  TriggerConditions);
            DCFG.Write(doc, os, "CompleteConditions", CompleteConditions);
            DCFG.Write(doc, os, "Effects",            Effects);
        }
    }
}
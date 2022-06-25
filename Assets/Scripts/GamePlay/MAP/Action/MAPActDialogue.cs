using UnityEngine;
using System.Collections;
using CFG;

namespace MAP
{
    [MAPActionClass("任务/显示对话", false)]
    public class MAPActDialogue : MAPAction
    {
        [MAPFieldAttri] public int  StDialogueID = 1;
        [MAPFieldAttri] public int  EdDialogueID = 2;

        public override void Trigger()
        {
            base.Trigger();
            UIDialogue dialogueWindow = (UIDialogue)GTWindowManager.Instance.OpenWindow(EWindowID.UIDialogue);
            dialogueWindow.ShowDialogue(StDialogueID, EdDialogueID, true, Release);
        }

        public override void Release()
        {
            GTWindowManager.Instance.HideWindow(EWindowID.UIDialogue);
            base.Release();
        }

        public override void Import(DCFG cfg)
        {
            MapActDialogue data = cfg as MapActDialogue;
            this.ID              = data.ID;
            this.StDialogueID    = data.StDialogueID;
            this.EdDialogueID    = data.EdDialogueID;
        }

        public override DCFG Export()
        {
            MapActDialogue data = new MapActDialogue();
            data.ID              = this.ID;
            data.StDialogueID    = this.StDialogueID;
            data.EdDialogueID    = this.EdDialogueID;
            return data;
        }
    }
}
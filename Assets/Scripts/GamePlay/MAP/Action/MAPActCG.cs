using UnityEngine;
using System.Collections;
using System;
using CFG;

namespace MAP
{
    [MAPActionClass("动画/视频动画", false)]
    public class MAPActCG : MAPAction
    {
        [MAPFieldAttri] public Int32 VideoID = 1;

        public override void Trigger()
        {
            base.Trigger();
            GTWorld.Instance.Video.Play(VideoID, Release);
        }

        public override void Import(DCFG cfg)
        {
            MapActCG data = cfg as MapActCG;
            this.ID        = data.ID;
            this.VideoID   = data.VideoID;
        }

        public override DCFG Export()
        {
            MapActCG data = new MapActCG();
            data.ID        = this.ID;
            data.VideoID   = this.VideoID;
            return data;
        }
    }
}
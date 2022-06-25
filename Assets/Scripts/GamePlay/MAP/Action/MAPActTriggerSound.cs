using UnityEngine;
using System.Collections;
using CFG;

namespace MAP
{
    [MAPActionClass("触发/声音", false)]
    public class MAPActTriggerSound : MAPAction
    {
        [MAPFieldAttri] public string AssetPath = string.Empty;
        [MAPFieldAttri] public float  Volume    = 1;
        [MAPFieldAttri] public float  Pitch     = 1;

        public override void Trigger()
        {
            base.Trigger();
            GTAudioManager.Instance.PlayEffectAudio(AssetPath, Volume, Pitch);
            this.Release();
        }

        public override void Import(DCFG cfg)
        {
            MapActTriggerSound data = cfg as MapActTriggerSound;
            this.ID                    = data.ID;
            this.AssetPath             = data.AssetPath;
            this.Volume                = data.Volume;
            this.Pitch                 = data.Pitch;
        }

        public override DCFG Export()
        {
            MapActTriggerSound data = new MapActTriggerSound();
            data.ID                    = this.ID;
            data.AssetPath             = this.AssetPath;
            data.Volume                = this.Volume;
            data.Pitch                 = this.Pitch;
            return data;
        }
    }
}
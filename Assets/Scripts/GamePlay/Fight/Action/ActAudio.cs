using UnityEngine;
using System.Collections;
using System;

namespace ACT
{
    [ActActionClass("动作", "声音", "")]
    public class ActAudio : ActItem
    {
        public string Path    = string.Empty;
        public float  Volume  = 1;
        public float  Pitch   = 1;
        public bool   Looped = false;

        private AudioSource m_TempSource;

        protected override bool Trigger()
        {
            this.m_TempSource = GTAudioManager.Instance.PlayEffectAudio(Path, Volume, Pitch, Looped);
            return true;
        }

        protected override void Release()
        {
            if (Looped)
            {
                GTAudioManager.Instance.EnqueueEffectAudio(this.m_TempSource);
                this.m_TempSource = null;
            }
        }
        public override ActItem Clone()
        {
            return this;
        }
    }
}
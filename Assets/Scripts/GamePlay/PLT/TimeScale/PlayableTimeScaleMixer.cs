using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System;

namespace PLT
{
    public class PlayableTimeScaleMixer : PlayableBehaviour
    {
        private float m_OldTimeScale = 1f;

        public override void OnGraphStart(Playable playable)
        {
            this.m_OldTimeScale = Time.timeScale;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            Int32 inputCount = playable.GetInputCount();
            float mixedTimeScale = 0f;
            float totalWeight = 0f;
            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                totalWeight += inputWeight;
                ScriptPlayable<PlayableTimeScale> playableInput = (ScriptPlayable<PlayableTimeScale>)playable.GetInput(i);
                PlayableTimeScale input = playableInput.GetBehaviour();
                mixedTimeScale += inputWeight * input.timeScale;
            }
            Time.timeScale = mixedTimeScale + m_OldTimeScale * (1f - totalWeight);
        }

        public override void OnGraphStop(Playable playable)
        {
            Time.timeScale = m_OldTimeScale;
        }
    }
}
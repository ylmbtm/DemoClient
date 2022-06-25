using UnityEngine;
using System.Collections;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System;

namespace PLT
{
    public class PlayableParentMixer : PlayableBehaviour
    {
        private Transform        m_TrackBinding;
        private PlayableDirector m_Director;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            m_TrackBinding = playerData as Transform;
            if (m_TrackBinding == null)
            {
                return;
            }
            if (m_Director == null)
            {
                m_Director = m_TrackBinding.GetComponentInParent<PlayableDirector>();
            }
            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                ScriptPlayable<PlayableParent> inputPlayable = (ScriptPlayable<PlayableParent>)playable.GetInput(i);
                PlayableParent input = inputPlayable.GetBehaviour();
                if (inputWeight > 0.5f)
                {
                    if(input.parent == null)
                    {
                        if (m_Director != null)
                        {
                            m_TrackBinding.parent = m_Director.transform;
                        }
                    }
                    else
                    {
                        m_TrackBinding.parent = input.parent;
                        m_TrackBinding.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    }
                    return;
                }
            }

            if (m_Director != null)
            {
                m_TrackBinding.parent = m_Director.transform;
            }
        }


        public override void OnGraphStop(Playable playable)
        {
            if (m_TrackBinding == null)
            {
                return;
            }
            if (m_Director != null)
            {
                m_TrackBinding.parent = m_Director.transform;
            }
        }
    }
}
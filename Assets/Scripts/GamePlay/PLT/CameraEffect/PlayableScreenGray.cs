using UnityEngine;
using System.Collections;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System;
using Cinemachine;

namespace PLT
{
    [Serializable]
    public class PlayableScreenGray : BasicPlayableBehaviour
    {
        public CinemachineBrain Brain;

        public override void OnGraphStart(Playable playable)
        {
            if (Brain != null)
            {
                ScreenGrayScale screenGrayScale = Brain.gameObject.GET<ScreenGrayScale>();
            }
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (Brain != null)
            {
                ScreenGrayScale screenGrayScale = Brain.gameObject.GET<ScreenGrayScale>();
                screenGrayScale.enabled = true;
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (Brain != null)
            {
                ScreenGrayScale screenGrayScale = Brain.gameObject.GET<ScreenGrayScale>();
                screenGrayScale.enabled = false;
            }
        }


        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            Brain = owner.GetComponentInChildren<CinemachineBrain>();
            return base.CreatePlayable(graph, owner);
        }
    }
}

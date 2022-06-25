using UnityEngine;
using System.Collections;
using UnityEngine.Timeline;
using Cinemachine;
using UnityEngine.Playables;

namespace PLT
{
    public class PlayableScreenRapidBlur : BasicPlayableBehaviour
    {
        public CinemachineBrain Brain;

        public override void OnGraphStart(Playable playable)
        {
            if (Brain != null)
            {
                ScreenRapidBlurEffect screenMotionBlurEffect = Brain.gameObject.GET<ScreenRapidBlurEffect>();
            }
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (Brain != null)
            {
                ScreenRapidBlurEffect screenMotionBlurEffect = Brain.gameObject.GET<ScreenRapidBlurEffect>();
                screenMotionBlurEffect.enabled = true;
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (Brain != null)
            {
                ScreenRapidBlurEffect screenMotionBlurEffect = Brain.gameObject.GET<ScreenRapidBlurEffect>();
                screenMotionBlurEffect.enabled = false;
            }
        }


        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            Brain = owner.GetComponentInChildren<CinemachineBrain>();
            return base.CreatePlayable(graph, owner);
        }
    }
}

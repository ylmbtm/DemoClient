using UnityEngine;
using System.Collections;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace PLT
{
    [System.Serializable]
    public class PlayableScreenText : BasicPlayableBehaviour
    {
        public string            content = string.Empty;
        public PlotTextComponent component;

        public override void OnGraphStart(Playable playable)
        {
            if (component != null)
            {
                component.content = content;
            }
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (component != null)
            {
                component.content = content;
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (component != null)
            {
                component.content = string.Empty;
            }
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (component != null)
            {
                component.content = content;
            }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            component = owner.GET<PlotTextComponent>();
            return base.CreatePlayable(graph, owner);
        }
    }
}
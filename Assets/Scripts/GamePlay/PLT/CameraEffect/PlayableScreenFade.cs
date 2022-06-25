using UnityEngine;
using System.Collections;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace PLT
{
    [System.Serializable]
    public class PlayableScreenFade : BasicPlayableBehaviour
    {
        public Color                 color     = Color.black;
        public AnimationCurve        fadeCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f), new Keyframe(3.0f, 1.0f), new Keyframe(4.0f, 0.0f));
        public PlotFadeComponent     component;

        public override void OnGraphStart(Playable playable)
        {
            if (component != null)
            {
                component.enabled = true;
            }
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (component != null)
            {
                component.enabled = true;
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (component != null)
            {
                component.enabled = false;
            }
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (component != null)
            {
                ScriptPlayable<PlayableScreenFade> scriptPlayable = (ScriptPlayable<PlayableScreenFade>)playable;
                float alpha = fadeCurve.Evaluate((float)scriptPlayable.GetTime());
                Color col = color;
                col.a = Mathf.Min(Mathf.Max(0.0f, alpha), 1.0f);
                component.enabled = true;
                component.color = col;
            }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            component = owner.GET<PlotFadeComponent>();
            return base.CreatePlayable(graph, owner);
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PLT
{
    [Serializable]
    public class PlayableTransformTweenClip : PlayableAsset, ITimelineClipAsset
    {
        public PlayableTransformTween      template = new PlayableTransformTween();
        public ExposedReference<Transform> startLocation;
        public ExposedReference<Transform> endLocation;

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<PlayableTransformTween>.Create(graph, template);
            PlayableTransformTween clone = playable.GetBehaviour();
            clone.startLocation = startLocation.Resolve(graph.GetResolver());
            clone.endLocation = endLocation.Resolve(graph.GetResolver());
            return playable;
        }
    }
}

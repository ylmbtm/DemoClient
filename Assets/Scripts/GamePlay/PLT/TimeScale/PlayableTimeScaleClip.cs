using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System;

namespace PLT
{
    [Serializable]
    public class PlayableTimeScaleClip : PlayableAsset, ITimelineClipAsset
    {
        public PlayableTimeScale template = new PlayableTimeScale();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Extrapolation | ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<PlayableTimeScale>.Create(graph, template);
        }
    }
}
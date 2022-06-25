using UnityEngine;
using System.Collections;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System;

namespace PLT
{
    [Serializable]
    public class PlayableLightClip : PlayableAsset, ITimelineClipAsset
    {
        public PlayableLight template = new PlayableLight();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<PlayableLight>.Create(graph, template); ;
        }
    }
}
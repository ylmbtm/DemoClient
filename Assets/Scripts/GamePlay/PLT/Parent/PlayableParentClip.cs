using UnityEngine;
using System.Collections;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System;

namespace PLT
{
    [Serializable]
    public class PlayableParentClip : PlayableAsset, ITimelineClipAsset
    {
        public PlayableParent              template         = new PlayableParent();
        public ExposedReference<Transform> parent;

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; } 
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            ScriptPlayable<PlayableParent> playable = ScriptPlayable<PlayableParent>.Create(graph, template);
            PlayableParent clone = playable.GetBehaviour();
            clone.parent = parent.Resolve(graph.GetResolver());
            return playable;

        }
    }
}
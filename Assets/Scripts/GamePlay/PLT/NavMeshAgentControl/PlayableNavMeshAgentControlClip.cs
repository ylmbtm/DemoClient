using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PLT
{
    [Serializable]
    public class PlayableNavMeshAgentControlClip : PlayableAsset, ITimelineClipAsset
    {
        public ExposedReference<Transform> destination;
        [HideInInspector]
        public PlayableNavMeshAgentControl template = new PlayableNavMeshAgentControl();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<PlayableNavMeshAgentControl>.Create(graph, template);
            PlayableNavMeshAgentControl clone = playable.GetBehaviour();
            clone.destination = destination.Resolve(graph.GetResolver());
            return playable;
        }
    }
}
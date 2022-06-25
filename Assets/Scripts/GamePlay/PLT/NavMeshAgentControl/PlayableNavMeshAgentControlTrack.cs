using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.AI;

namespace PLT
{
    [TrackColor(0.855f, 0.8623f, 0.87f)]
    [TrackClipType(typeof(PlayableNavMeshAgentControlClip))]
    [TrackBindingType(typeof(NavMeshAgent))]
    public class PlayableNavMeshAgentControlTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<PlayableNavMeshAgentControlMixer>.Create(graph, inputCount);
        }
    }
}

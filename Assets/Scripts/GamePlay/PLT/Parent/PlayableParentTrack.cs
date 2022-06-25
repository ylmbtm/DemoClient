using UnityEngine;
using System.Collections;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace PLT
{
    [TrackClipType(typeof(PlayableParentClip))]
    [TrackColor(1, 1, 1)]
    [TrackBindingType(typeof(Transform))]
    public class PlayableParentTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<PlayableParentMixer>.Create(graph, inputCount); 
        }
    }
}
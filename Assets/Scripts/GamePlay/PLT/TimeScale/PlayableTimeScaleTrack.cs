using UnityEngine;
using System.Collections;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace PLT
{
    [TrackColor(0.855f, 0.8623f, 0.87f)]
    [TrackClipType(typeof(PlayableTimeScaleClip))]
    public class PlayableTimeScaleTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<PlayableTimeScaleMixer>.Create(graph, inputCount);
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PLT
{
	[Serializable]
    [TrackClipType(typeof(PlayableVideoScriptAsset))]
    [TrackMediaType(TimelineAsset.MediaType.Script)]
    [TrackColor(0.008f, 0.698f, 0.655f)]
    public class PlayableVideoScriptTrack : TrackAsset
	{
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            PlayableDirector playableDirector = go.GetComponent<PlayableDirector>();

            ScriptPlayable<PlayableVideoSchedulerBehaviour> playable =
                ScriptPlayable<PlayableVideoSchedulerBehaviour>.Create(graph, inputCount);

            PlayableVideoSchedulerBehaviour videoSchedulerPlayableBehaviour =
                   playable.GetBehaviour();

            if (videoSchedulerPlayableBehaviour != null)
            {
                videoSchedulerPlayableBehaviour.director = playableDirector;
                videoSchedulerPlayableBehaviour.clips = GetClips();
            }

            return playable;
        }
    }
}


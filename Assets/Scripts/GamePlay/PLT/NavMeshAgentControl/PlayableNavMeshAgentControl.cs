using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PLT
{
    [Serializable]
    public class PlayableNavMeshAgentControl : PlayableBehaviour
    {
        public Transform destination;
        public bool      destinationSet;

        public override void OnGraphStart(Playable playable)
        {
            destinationSet = false;
        }
    }
}

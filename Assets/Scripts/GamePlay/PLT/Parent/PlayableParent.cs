using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System;

namespace PLT
{
    [Serializable]
    public class PlayableParent : PlayableBehaviour
    {
        public Transform parent;
    }
}
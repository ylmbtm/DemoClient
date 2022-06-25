using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System;

namespace PLT
{
    [Serializable]
    public class PlayableTimeScale : PlayableBehaviour
    {
        public float timeScale = 1f;
    }
}
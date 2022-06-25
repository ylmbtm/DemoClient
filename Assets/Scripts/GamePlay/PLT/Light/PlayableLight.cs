using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using System;

namespace PLT
{
    [Serializable]
    public class PlayableLight : PlayableBehaviour
    {
        public Color color           = Color.white;
        public float intensity       = 1f;
        public float bounceIntensity = 1f;
        public float range           = 10f;
    }
}

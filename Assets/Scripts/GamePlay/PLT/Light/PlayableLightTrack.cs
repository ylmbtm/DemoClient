using UnityEngine;
using System.Collections;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace PLT
{
    [TrackColor(0.9454092f, 0.9779412f, 0.3883002f)]
    [TrackClipType(typeof(PlayableLightClip))]
    [TrackBindingType(typeof(Light))]
    public class PlayableLightTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<PlayableLightMixer>.Create(graph, inputCount);
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
#if UNITY_EDITOR
            Light trackBinding = director.GetGenericBinding(this) as Light;
            if (trackBinding == null)
                return;

            var serializedObject = new UnityEditor.SerializedObject(trackBinding);
            var iterator = serializedObject.GetIterator();
            while (iterator.NextVisible(true))
            {
                if (iterator.hasVisibleChildren)
                    continue;

                driver.AddFromName<Light>(trackBinding.gameObject, iterator.propertyPath);
            }
#endif
            base.GatherProperties(director, driver);
        }
    }
}
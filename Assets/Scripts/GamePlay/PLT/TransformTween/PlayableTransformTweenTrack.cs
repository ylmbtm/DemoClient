using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PLT
{
    [TrackColor(0.855f, 0.8623f, 0.870f)]
    [TrackClipType(typeof(PlayableTransformTweenClip))]
    [TrackBindingType(typeof(Transform))]
    public class PlayableTransformTweenTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<PlayableTransformTweenMixer>.Create(graph, inputCount);
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
#if UNITY_EDITOR
            var comp = director.GetGenericBinding(this) as Transform;
            if (comp == null)
                return;
            var so = new UnityEditor.SerializedObject(comp);
            var iter = so.GetIterator();
            while (iter.NextVisible(true))
            {
                if (iter.hasVisibleChildren)
                    continue;
                driver.AddFromName<Transform>(comp.gameObject, iter.propertyPath);
            }
#endif
            base.GatherProperties(director, driver);
        }
    }
}

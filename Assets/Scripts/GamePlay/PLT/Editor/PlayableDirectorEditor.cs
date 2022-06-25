using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.Playables;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditorInternal;
using System.Reflection;
using UnityEditor.VersionControl;
using UnityEditor.SceneManagement;

namespace PLT
{
    [CanEditMultipleObjects, CustomEditor(typeof(PlayableDirector))]
    public class PlayableDirectorEditor : Editor
    {
        private static class Styles
        {
            public static readonly GUIContent PlayableText         = EDraw.TextContent("Playable");
            public static readonly GUIContent InitialTimeContent   = EDraw.TextContent("Initial Time|The time at which the Playable will begin playing");
            public static readonly GUIContent TimeContent          = EDraw.TextContent("Current Time|The current Playable time");
            public static readonly GUIContent InitialStateContent  = EDraw.TextContent("Play On Awake|Whether the Playable should be playing after it loads");
            public static readonly GUIContent UpdateMethod         = EDraw.TextContent("Update Method|Controls how the Playable updates every frame");
            public static readonly GUIContent WrapModeContent      = EDraw.TextContent("Wrap Mode|Controls the behaviour of evaluating the Playable outside its duration");
            public static readonly GUIContent NoBindingsContent    = EDraw.TextContent("This channel will not playback because it is not currently assigned");
            public static readonly GUIContent BindingsTitleContent = EDraw.TextContent("Bindings");
        }

        private struct BindingPropertyPair
        {
            public PlayableBinding    binding;
            public SerializedProperty property;
        }

        private SerializedProperty        m_PlayableAsset;
        private SerializedProperty        m_InitialState;
        private SerializedProperty        m_WrapMode;
        private SerializedProperty        m_InitialTime;
        private SerializedProperty        m_UpdateMethod;
        private SerializedProperty        m_SceneBindings;
        private GUIContent                m_AnimatorContent;
        private GUIContent                m_AudioContent;
        private GUIContent                m_ScriptContent;
        private Texture                   m_DefaultScriptContentTexture;
        private List<BindingPropertyPair> m_BindingPropertiesCache = new List<BindingPropertyPair>();
        private PlayableBinding[]         m_SynchedPlayableBindings = null;


        public void OnEnable()
        {
            this.m_PlayableAsset               = base.serializedObject.FindProperty("m_PlayableAsset");
            this.m_InitialState                = base.serializedObject.FindProperty("m_InitialState");
            this.m_WrapMode                    = base.serializedObject.FindProperty("m_WrapMode");
            this.m_UpdateMethod                = base.serializedObject.FindProperty("m_DirectorUpdateMode");
            this.m_InitialTime                 = base.serializedObject.FindProperty("m_InitialTime");
            this.m_SceneBindings               = base.serializedObject.FindProperty("m_SceneBindings");
            this.m_AnimatorContent             = new GUIContent(AssetPreview.GetMiniTypeThumbnail(typeof(Animator)));
            this.m_AudioContent                = new GUIContent(AssetPreview.GetMiniTypeThumbnail(typeof(AudioSource)));
            this.m_ScriptContent               = new GUIContent(EDraw.LoadIcon("ScriptableObject Icon"));
            this.m_DefaultScriptContentTexture = this.m_ScriptContent.image;
        }

        public override void OnInspectorGUI()
        {
            if (this.PlayableAssetOutputsChanged())
            {
                this.SynchSceneBindings();
            }
            base.serializedObject.Update();
            if (PlayableDirectorEditor.PropertyFieldAsObject(this.m_PlayableAsset, PlayableDirectorEditor.Styles.PlayableText, typeof(PlayableAsset), false, false))
            {
                base.serializedObject.ApplyModifiedProperties();
                this.SynchSceneBindings();
                InternalEditorUtility.RepaintAllViews();
            }
            EditorGUILayout.PropertyField(this.m_UpdateMethod, PlayableDirectorEditor.Styles.UpdateMethod, new GUILayoutOption[0]);
            Rect controlRect = EditorGUILayout.GetControlRect(true, new GUILayoutOption[0]);
            GUIContent label = EditorGUI.BeginProperty(controlRect, PlayableDirectorEditor.Styles.InitialStateContent, this.m_InitialState);
            bool flag = this.m_InitialState.enumValueIndex != 0;
            EditorGUI.BeginChangeCheck();
            flag = EditorGUI.Toggle(controlRect, label, flag);
            if (EditorGUI.EndChangeCheck())
            {
                this.m_InitialState.enumValueIndex = ((!flag) ? 0 : 1);
            }
            EditorGUI.EndProperty();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this.m_WrapMode, PlayableDirectorEditor.Styles.WrapModeContent, new GUILayoutOption[0]);
            if (EditorGUI.EndChangeCheck())
            {
                DirectorWrapMode enumValueIndex = (DirectorWrapMode)this.m_WrapMode.enumValueIndex;
                foreach (PlayableDirector current in base.targets.OfType<PlayableDirector>())
                {
                    current.extrapolationMode = enumValueIndex;
                }
            }
            PlayableDirectorEditor.PropertyFieldAsFloat(this.m_InitialTime, PlayableDirectorEditor.Styles.InitialTimeContent);
            if (Application.isPlaying)
            {
                this.CurrentTimeField();
            }
            if (base.targets.Length == 1)
            {
                PlayableAsset x = this.m_PlayableAsset.objectReferenceValue as PlayableAsset;
                if (x != null)
                {
                    this.DoDirectorBindingInspector();
                }
            }

            PlayableDirector director = target as PlayableDirector;
            if (PrefabUtility.GetPrefabType(director.gameObject) == PrefabType.PrefabInstance)
            {
                GUILayout.Space(5);
                if (GUILayout.Button("Apply", EGUIStyles.Button3))
                {
                    UnityEngine.Object prefabParent = PrefabUtility.GetPrefabParent(director.gameObject);
                    string assetPath = AssetDatabase.GetAssetPath(prefabParent);
                    PrefabUtility.ReplacePrefab(director.gameObject, prefabParent, ReplacePrefabOptions.ConnectToPrefab);
                    EditorSceneManager.MarkSceneDirty(director.gameObject.scene);
                    GUIUtility.ExitGUI();
                }
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("启动黑边", EGUIStyles.Button3))
                {
                    director.gameObject.GET<PlotTextComponent>().isEnableBlack = true;
                    InternalEditorUtility.RepaintAllViews();
                }
                GUILayout.Space(5);
                if (GUILayout.Button("禁用黑边", EGUIStyles.Button3))
                {
                    director.gameObject.GET<PlotTextComponent>().isEnableBlack = false;
                    InternalEditorUtility.RepaintAllViews();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            base.serializedObject.ApplyModifiedProperties();
        }

        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying;
        }

        private bool PlayableAssetOutputsChanged()
        {
            PlayableAsset playableAsset = this.m_PlayableAsset.objectReferenceValue as PlayableAsset;
            bool result;
            if (this.m_SynchedPlayableBindings == null)
            {
                result = (playableAsset != null);
            }
            else
            {
                result = (playableAsset == null || playableAsset.outputs.Count<PlayableBinding>() != this.m_SynchedPlayableBindings.Length || playableAsset.outputs.Where((PlayableBinding t, int i) => t.sourceObject != this.m_SynchedPlayableBindings[i].sourceObject).Any<PlayableBinding>());
            }
            return result;
        }

        private void BindingInspector(SerializedProperty bindingProperty, PlayableBinding binding)
        {
            if (!(binding.sourceObject == null))
            {
                UnityEngine.Object objectReferenceValue = bindingProperty.objectReferenceValue;
                if (binding.streamType == DataStreamType.Audio)
                {
                    this.m_AudioContent.text = binding.streamName;
                    this.m_AudioContent.tooltip = ((!(objectReferenceValue == null)) ? string.Empty : PlayableDirectorEditor.Styles.NoBindingsContent.text);
                    PlayableDirectorEditor.PropertyFieldAsObject(bindingProperty, this.m_AudioContent, typeof(AudioSource), false, false);
                }
                else
                {
                    if (binding.streamType == DataStreamType.Animation)
                    {
                        this.m_AnimatorContent.text = binding.streamName;
                        this.m_AnimatorContent.tooltip = ((!(objectReferenceValue is GameObject)) ? string.Empty : PlayableDirectorEditor.Styles.NoBindingsContent.text);
                        PlayableDirectorEditor.PropertyFieldAsObject(bindingProperty, this.m_AnimatorContent, typeof(Animator), true, true);
                    }
                    else
                    {
                        if (binding.streamType == DataStreamType.None)
                        {
                            this.m_ScriptContent.text = binding.streamName;
                            this.m_ScriptContent.tooltip = ((!(objectReferenceValue == null)) ? string.Empty : PlayableDirectorEditor.Styles.NoBindingsContent.text);
                            this.m_ScriptContent.image = (AssetPreview.GetMiniTypeThumbnail(binding.sourceBindingType) ?? this.m_DefaultScriptContentTexture);
                            if (binding.sourceBindingType != null && typeof(UnityEngine.Object).IsAssignableFrom(binding.sourceBindingType))
                            {
                                PlayableDirectorEditor.PropertyFieldAsObject(bindingProperty, this.m_ScriptContent, binding.sourceBindingType, true, false);
                            }
                        }
                    }
                }
            }
        }

        private void DoDirectorBindingInspector()
        {
            if (this.m_BindingPropertiesCache.Any<PlayableDirectorEditor.BindingPropertyPair>())
            {
                this.m_SceneBindings.isExpanded = EditorGUILayout.Foldout(this.m_SceneBindings.isExpanded, PlayableDirectorEditor.Styles.BindingsTitleContent);
                if (this.m_SceneBindings.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    foreach (PlayableDirectorEditor.BindingPropertyPair current in this.m_BindingPropertiesCache)
                    {
                        this.BindingInspector(current.property, current.binding);
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }

        private void SynchSceneBindings()
        {
            if (base.targets.Length <= 1)
            {
                PlayableDirector playableDirector = (PlayableDirector)base.target;
                PlayableAsset playableAsset = this.m_PlayableAsset.objectReferenceValue as PlayableAsset;
                this.m_BindingPropertiesCache.Clear();
                this.m_SynchedPlayableBindings = null;
                if (!(playableAsset == null))
                {
                    IEnumerable<PlayableBinding> outputs = playableAsset.outputs;
                    this.m_SynchedPlayableBindings = outputs.ToArray<PlayableBinding>();
                    PlayableBinding[] synchedPlayableBindings = this.m_SynchedPlayableBindings;
                    for (int i = 0; i < synchedPlayableBindings.Length; i++)
                    {
                        PlayableBinding playableBinding = synchedPlayableBindings[i];
                        if (playableDirector.GetGenericBinding(playableBinding.sourceObject) == false)
                        {
                            playableDirector.SetGenericBinding(playableBinding.sourceObject, null);
                        }
                    }
                    base.serializedObject.Update();
                    PlayableBinding[] synchedPlayableBindings2 = this.m_SynchedPlayableBindings;
                    for (int j = 0; j < synchedPlayableBindings2.Length; j++)
                    {
                        PlayableBinding binding = synchedPlayableBindings2[j];
                        for (int k = 0; k < this.m_SceneBindings.arraySize; k++)
                        {
                            SerializedProperty arrayElementAtIndex = this.m_SceneBindings.GetArrayElementAtIndex(k);
                            if (arrayElementAtIndex.FindPropertyRelative("key").objectReferenceValue == binding.sourceObject)
                            {
                                this.m_BindingPropertiesCache.Add(new PlayableDirectorEditor.BindingPropertyPair
                                {
                                    binding = binding,
                                    property = arrayElementAtIndex.FindPropertyRelative("value")
                                });
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void CurrentTimeField()
        {
            if (base.targets.Length == 1)
            {
                PlayableDirector playableDirector = (PlayableDirector)base.target;
                EditorGUI.BeginChangeCheck();
                float num = EditorGUILayout.FloatField(PlayableDirectorEditor.Styles.TimeContent, (float)playableDirector.time, new GUILayoutOption[0]);
                if (EditorGUI.EndChangeCheck())
                {
                    playableDirector.time = (double)num;
                }
            }
            else
            {
                EditorGUILayout.TextField(PlayableDirectorEditor.Styles.TimeContent, mixedValueContent.text, new GUILayoutOption[0]);
            }
        }

        private static void PropertyFieldAsFloat(SerializedProperty property, GUIContent title)
        {
            Rect controlRect = EditorGUILayout.GetControlRect(new GUILayoutOption[0]);
            title = EditorGUI.BeginProperty(controlRect, title, property);
            EditorGUI.BeginChangeCheck();
            float num = EditorGUI.FloatField(controlRect, title, (float)property.doubleValue);
            if (EditorGUI.EndChangeCheck())
            {
                property.doubleValue = (double)num;
            }
            EditorGUI.EndProperty();
        }

        private static bool PropertyFieldAsObject(SerializedProperty property, GUIContent title, Type objType, bool allowSceneObjects, bool useBehaviourGameObject = false)
        {
            Rect controlRect = EditorGUILayout.GetControlRect(new GUILayoutOption[0]);
            GUIContent label = EditorGUI.BeginProperty(controlRect, title, property);
            EditorGUI.BeginChangeCheck();
            UnityEngine.Object @object = EditorGUI.ObjectField(controlRect, label, property.objectReferenceValue, objType, allowSceneObjects);
            bool flag = EditorGUI.EndChangeCheck();
            if (flag)
            {
                if (useBehaviourGameObject)
                {
                    Behaviour behaviour = @object as Behaviour;
                    property.objectReferenceValue = ((!(behaviour != null)) ? null : behaviour.gameObject);
                }
                else
                {
                    property.objectReferenceValue = @object;
                }
            }
            EditorGUI.EndProperty();
            return flag;
        }

        private static GUIContent mixedValueContent
        {
            get
            {
                var ty = typeof(UnityEditor.EditorGUI);
                var mi = ty.GetProperty("mixedValueContent", BindingFlags.NonPublic | BindingFlags.Static);
                return mi.GetValue(null, new object[0] { }) as GUIContent;
            }
        }
    }
}

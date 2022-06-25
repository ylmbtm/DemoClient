using UnityEngine;
using System.Collections;
using UnityEngine.Video;
using System;

namespace CUT
{
    public class VideoSystem
    {
        private VideoPlayer              m_VideoPlayer;
        private System.Action            m_OnFinish;

        public void Play(int id, Action onFinish = null)
        {
            DVideo db = ReadCfgVideo.GetDataById(id);
            if (db == null)
            {
                return;
            }
            if (m_VideoPlayer == null)
            {
                m_VideoPlayer = CreateVideoPlayer();
            }
            VideoClip clip = GTResourceManager.Instance.Load<VideoClip>(db.Path);
            m_OnFinish = onFinish;
            m_VideoPlayer.clip = clip;
            m_VideoPlayer.loopPointReached += OnFinish;
            m_VideoPlayer.Play();
            GTWindowManager.Instance.LockNGUI(true);
        }

        public void Stop()
        {
            if (m_OnFinish != null)
            {
                m_OnFinish();
                m_OnFinish = null;
            }
            if (m_VideoPlayer != null)
            {
                m_VideoPlayer.Stop();
                m_VideoPlayer.gameObject.SetActive(false);
            }
            GTWindowManager.Instance.LockNGUI(false);
        }

        void        OnFinish(VideoPlayer videoPlayer)
        {
            if (m_OnFinish != null)
            {
                m_OnFinish();
                m_OnFinish = null;
            }
            if (m_VideoPlayer != null)
            {
                m_VideoPlayer.gameObject.SetActive(false);
            }
            GTWindowManager.Instance.LockNGUI(false);
        }

        VideoPlayer CreateVideoPlayer()
        {
            GameObject go = new GameObject("VideoPlayer");
            VideoPlayer videoPlayer = go.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            videoPlayer.targetCamera = GTCameraManager.Instance.NGUICamera;
            videoPlayer.targetCameraAlpha = 1;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, GTAudioManager.Instance.MusicAudioSource);
            return videoPlayer;
        }
    }
}


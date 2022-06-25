using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GTAudioManager : GTMonoSingleton<GTAudioManager>
{
    public enum AudioChannel
    {
        Motion       = 1,
        Action       = 2,
        Skill        = 4,
        Behit        = 8,
        SkillCombine = 16
    }

    public bool               MusicActive                          { get; private set; }
    public bool               SoundActive                          { get; private set; }
    public AudioSource        MusicAudioSource                     { get; private set; }
    public AudioSource        SoundAudioSource                     { get; private set; }

    private Queue<AudioSource> m_EffectAudioSourceQueue;
    private GameObject         m_MusicSource;
    private GameObject         m_SoundSource;

    public override void SetRoot(Transform parent)
    {
        base.SetRoot(parent);
        m_MusicSource = new GameObject("MusicSource");
        m_SoundSource = new GameObject("SoundSource");
        m_MusicSource.transform.parent = transform;
        m_SoundSource.transform.parent = transform;
        MusicAudioSource = m_MusicSource.AddComponent<AudioSource>();
        MusicAudioSource.loop = true;
        MusicAudioSource.playOnAwake = false;
        SoundAudioSource = m_SoundSource.AddComponent<AudioSource>();
        SoundAudioSource.loop = false;
        SoundAudioSource.playOnAwake = false;
        MusicActive = GTData.NativeData.MusicActive;
        SoundActive = GTData.NativeData.SoundActive;
        m_EffectAudioSourceQueue = new Queue<AudioSource>();
        for (int i = 0; i < 20; i++)
        {
            AudioSource ad = m_SoundSource.AddComponent<AudioSource>();
            m_EffectAudioSourceQueue.Enqueue(ad);
        }
    }

    public void SetMusicActive(bool active)
    {
        if (MusicActive == active)
        {
            return;
        }
        MusicActive = active;
        GTData.NativeData.MusicActive = active;
        if (MusicAudioSource == null)
        {
            return;
        }
        MusicAudioSource.gameObject.SetActive(active);
    }

    public void SetSoundActive(bool active)
    {
        if (SoundActive == active)
        {
            return;
        }
        if (SoundAudioSource == null)
        {
            return;
        }
        SoundActive = active;
        GTData.NativeData.SoundActive = active;
    }

    public void PlayClipAtPoint(string soundPath, Vector3 pos)
    {
        if (SoundActive == false)
        {
            return;
        }
        AudioClip clip = GTResourceManager.Instance.Load<AudioClip>(soundPath);
        if (clip == null)
        {
            return;
        }
        AudioSource.PlayClipAtPoint(clip, pos);
    }

    public AudioSource PlaySound(string soundPath)
    {
        if (SoundActive == false)
        {
            return null;
        }
        AudioClip clip = GTResourceManager.Instance.Load<AudioClip>(soundPath);
        if (clip == null)
        {
            return null;
        }
        SoundAudioSource.Stop();
        SoundAudioSource.clip = clip;
        SoundAudioSource.PlayOneShot(clip);
        return SoundAudioSource;
    }

    public AudioSource PlayEffectAudio(string soundPath)
    {
        if (SoundActive == false)
        {
            return null;
        }
        AudioClip clip = GTResourceManager.Instance.Load<AudioClip>(soundPath);
        if (clip == null)
        {
            return null;
        }
        AudioSource audio = DequeueEffectAudio();
        audio.clip = clip;
        audio.volume = 1;
        audio.Play();
        audio.loop = false;
        m_EffectAudioSourceQueue.Enqueue(audio);
        return audio;
    }

    public AudioSource PlayEffectAudio(string soundPath, float volume = 1f, float pitch = 1f, bool loop = false)
    {
        if (SoundActive == false)
        {
            return null;
        }
        AudioClip clip = GTResourceManager.Instance.Load<AudioClip>(soundPath);
        if (clip == null)
        {
            return null;
        }
        AudioSource audio = DequeueEffectAudio();
        audio.clip = clip;
        audio.volume = volume;
        audio.pitch = pitch;
        audio.loop = loop;
        audio.Play();
        if (!loop)
        {
            m_EffectAudioSourceQueue.Enqueue(audio);
        }
        return audio;
    }

    public AudioSource EnqueueEffectAudio(AudioSource audio)
    {
        if (audio == null)
        {
            return null;
        }
        audio.volume = 0;
        audio.clip = null;
        audio.Stop();
        m_EffectAudioSourceQueue.Enqueue(audio);
        return audio;
    }

    public AudioSource DequeueEffectAudio()
    {
        if (m_EffectAudioSourceQueue.Count == 0)
        {
            AudioSource ad = m_SoundSource.AddComponent<AudioSource>();
            return ad;
        }
        else
        {
            return m_EffectAudioSourceQueue.Dequeue();
        }
    }

    public AudioSource PlayMusic(string soundPath)
    {
        if (MusicActive == false)
        {
            return null;
        }
        AudioClip clip = GTResourceManager.Instance.Load<AudioClip>(soundPath);
        if (clip == null)
        {
            return null;
        }
        if (MusicAudioSource.isPlaying && MusicAudioSource.clip.name == clip.name)
        {
            return null;
        }
        MusicAudioSource.gameObject.SetActive(true);
        MusicAudioSource.clip = clip;
        MusicAudioSource.loop = true;
        MusicAudioSource.Play();
        return MusicAudioSource;
    }
}

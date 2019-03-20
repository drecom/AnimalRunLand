using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Liver
{
public class BgmManager : MonoBehaviour
{
    static BgmManager instance;
    static bool dontDestroy = false;
    public Sound.BgmKind bgmKind = Sound.BgmKind.TOP;

    public static BgmManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<BgmManager>();

                if (instance == null)
                {
                    GameObject newObject = new GameObject(typeof(BgmManager).ToString());
                    instance = newObject.AddComponent<BgmManager>();
                }

                if (!dontDestroy)
                {
                    DontDestroyOnLoad(instance.gameObject);
                    dontDestroy = true;
                }
            }

            return instance;
        }
    }

    void Awake()
    {
        for (var i = 0; i < 2; i++)
        {
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSources[i] = new MasterVolumeAudioSource()
            {
                Source = audioSource,
                GetMasterVolume = () => Volume,
            };
        }

        MasterVolume = 1.0f;
    }

    MasterVolumeAudioSource[] audioSources = new MasterVolumeAudioSource[2];
    Coroutine[] audioSourceEnumerators = new Coroutine[2];
    int current = 0;

    MasterVolumeAudioSource CurrentAudioSource
    {
        get { return audioSources[current]; }
        set { audioSources[current] = value; }
    }
    MasterVolumeAudioSource OtherAudioSource
    {
        get { return audioSources[1 - current]; }
        set { audioSources[1 - current] = value; }
    }
    Coroutine CurrentAudioSourceEnumerator
    {
        get { return audioSourceEnumerators[current]; }
        set { audioSourceEnumerators[current] = value; }
    }
    Coroutine OtherAudioSourceEnumerator
    {
        get { return audioSourceEnumerators[1 - current]; }
        set { audioSourceEnumerators[1 - current] = value; }
    }

    IEnumerator Fade(MasterVolumeAudioSource source, float toVolume, float timeSeconds, float volume)
    {
        var startTime = Time.time;
        var startVolume = source.Volume;

        while (true)
        {
            var spentTime = Time.time - startTime;
            var t = spentTime / timeSeconds;

            if (t >= 1.0f)
            {
                //source.Volume = toVolume;
                source.Volume = volume;

                if (volume == 0.0f)
                {
                    source.Source.Stop();
                }

                yield break;
            }

            source.Volume = Mathf.Lerp(startVolume, toVolume, t);
            yield return null;
        }
    }

    float masterVolume;
    public float MasterVolume
    {
        get { return masterVolume; }
        set
        {
            masterVolume = Mathf.Clamp(value, 0.0f, 1.0f);

            foreach (var audioSource in audioSources)
            {
                audioSource.UpdateVolume();
            }
        }
    }

    public float GetVolume(string name)
    {
        if (!volumeDic.ContainsKey(name))
        {
            return 1.0f;
        }

        return volumeDic[name];
    }
    public void SetVolume(string name, float volume)
    {
        volumeDic[name] = Mathf.Clamp(volume, 0.0f, 1.0f);

        foreach (var audioSource in audioSources)
        {
            audioSource.UpdateVolume();
        }
    }

    Dictionary<string, float> volumeDic = new Dictionary<string, float>();
    public float Volume
    {
        get { return masterVolume * volumeDic.Values.Aggregate(1.0f, (a, b) => a * b); }
    }

    public Func<string, AudioClip> LoadAudioClip = bgmName => Resources.Load(bgmName) as AudioClip;

    public void Play(Sound.BgmKind kind, float fadeInTimeSeconds, float fadeOutTimeSeconds, bool loop, float volume, System.Action onPlayStart = null)
    {
        this.bgmKind = kind;
        var faileName = "Sound/Bgm/" + kind.ToString();
        var audioClip = LoadAudioClip(faileName);

        if (audioClip == null)
        {
            Debug.LogError(string.Format("SE {0} は存在しません。", kind.ToString()));
            return;
        }

        // fade out
        if (CurrentAudioSourceEnumerator != null)
        {
            StopCoroutine(CurrentAudioSourceEnumerator);
        }

        if (CurrentAudioSource != null)
        {
            CurrentAudioSourceEnumerator = StartCoroutine(Fade(CurrentAudioSource, 0.0f, fadeOutTimeSeconds, 0.0f));
        }

        // fade in
        OtherAudioSource.Source.Stop();
        OtherAudioSource.Source.clip = audioClip;
        OtherAudioSource.Volume = 0.0f;
        OtherAudioSource.Source.loop = loop;
        OtherAudioSource.Source.Play();

        if (onPlayStart != null) { onPlayStart(); }

        if (OtherAudioSourceEnumerator != null)
        {
            StopCoroutine(OtherAudioSourceEnumerator);
        }

        OtherAudioSourceEnumerator = StartCoroutine(Fade(OtherAudioSource, 0.3f, fadeInTimeSeconds, volume));
        current = 1 - current;
    }

    public void Stop(float fadeOutTimeSeconds)
    {
        if (CurrentAudioSourceEnumerator != null)
        {
            if (fadeOutTimeSeconds == 0.0f)
            {
                CurrentAudioSource.Volume = 0.0f;
            }
            else
            {
                StartCoroutine(Fade(CurrentAudioSource, 0.0f, fadeOutTimeSeconds, 0.0f));
            }
        }
    }
}
}

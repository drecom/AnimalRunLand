using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SeManager : MonoBehaviour
{
    static SeManager instance;
    static bool dontDestroy = false;

    public static SeManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<SeManager>();

                if (instance == null)
                {
                    GameObject newObject = new GameObject(typeof(SeManager).ToString());
                    instance = newObject.AddComponent<SeManager>();
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
        for (var i = 0; i < MAX_SE; i++)
        {
            // 3Dサウンド用に子供を作成
            var obj = new GameObject("SE");
            obj.transform.SetParent(transform);
            var audioSource = obj.AddComponent<AudioSource>();
            audioSources[i] = new MasterVolumeAudioSource()
            {
                Source          = audioSource,
                Transform       = obj.transform,
                GetMasterVolume = () => Volume,
            };
        }

        MasterVolume = 1.0f;
    }

    const int MAX_SE = 32;
    MasterVolumeAudioSource[] audioSources = new MasterVolumeAudioSource[MAX_SE];
    int current = 0;

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
    List<AudioClip> cacheAudioClip = new List<AudioClip>();

    AudioClip LoadAudioClip(string seName)
    {
        var res = cacheAudioClip.Find(clip => seName.EndsWith(clip.name));

        if (res != null)
        {
            // 末尾に移動
            cacheAudioClip.Remove(res);
            cacheAudioClip.Add(res);
        }
        else
        {
            res = Resources.Load(seName) as AudioClip;
            cacheAudioClip.Add(res);

            // キャッシュ上限を超えたら、先頭の一番参照されてないものを削除する
            if (cacheAudioClip.Count > 10)
            {
                cacheAudioClip.RemoveAt(0);
            }
        }

        return res;
    }

    IEnumerator Fade(MasterVolumeAudioSource source, float toVolume, float timeSeconds)
    {
        var startTime = Time.time;
        var startVolume = source.Volume;

        while (true)
        {
            var spentTime = Time.time - startTime;
            var t = spentTime / timeSeconds;

            if (t >= 1.0f)
            {
                source.Volume = toVolume;

                if (Mathf.Approximately(toVolume, 0.0f))
                {
                    // ループ解除
                    // NOTE 解除しないと再生用のハンドルが再利用できない
                    source.Source.loop = false;
                }

                yield break;
            }

            source.Volume = Mathf.Lerp(startVolume, toVolume, t);
            yield return null;
        }
    }
    public int? Play(string seName, float fadeInTimeSeconds, bool loop, float speed)
    {
        //Debug.LogError(string.Format("★★★se:{0}  date:{1}:{2}:{3}", seName, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second));
        var faileName = "Sound/Se/" + seName;
        var audioClip = LoadAudioClip(faileName);

        if (audioClip == null)
        {
            Debug.LogError(string.Format("SE {0} は存在しません。", seName));
            return null;
        }

        // fade in
        var audioSource = audioSources[current];
        audioSource.Source.Stop();
        audioSource.Source.clip = audioClip;
        audioSource.Volume = 0.0f;
        audioSource.Source.loop = loop;
        audioSource.Source.pitch = speed;
        audioSource.Source.spatialBlend = 0;
        audioSource.Source.Play();
        StartCoroutine(Fade(audioSource, 1.0f, fadeInTimeSeconds));
        var res = current;

        do
        {
            current = (current + 1) % audioSources.Length;
        }
        while (audioSources[current].Source.loop);

        return res;
    }


    // 3D用再生
    public int? Play(string seName, float fadeInTimeSeconds, bool loop, float speed, Transform transform)
    {
        //Debug.LogError(string.Format("★★★se:{0}  date:{1}:{2}:{3}", seName, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second));
        var faileName = "Sound/Se/" + seName;
        var audioClip = LoadAudioClip(faileName);

        if (audioClip == null)
        {
            Debug.LogError(string.Format("SE {0} は存在しません。", seName));
            return null;
        }

        // fade in
        var audioSource = audioSources[current];
        audioSource.Source.Stop();
        audioSource.Source.clip = audioClip;
        audioSource.Transform.position = transform.position;
        audioSource.Volume = 0.0f;
        audioSource.Source.loop = loop;
        audioSource.Source.pitch = speed;
        audioSource.Source.spatialBlend = 0.5f;
        audioSource.Source.Play();
        StartCoroutine(Fade(audioSource, 1.0f, fadeInTimeSeconds));
        var res = current;

        do
        {
            current = (current + 1) % audioSources.Length;
        }
        while (audioSources[current].Source.loop);

        return res;
    }


    public void SetVolume(bool isOn)
    {
        var audioSource = audioSources[current];
        audioSource.Volume = isOn ? 1.0f : 0.0f;
    }

    public void Stop(float fadeOutTimeSeconds)
    {
        foreach (var audioSource in audioSources)
        {
            StartCoroutine(Fade(audioSource, 0.0f, fadeOutTimeSeconds));
        }
    }

    /// <summary>
    /// handle 指定して、停止する
    /// 主に Loop SE に使用します
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="fadeOutTimeSeconds"></param>
    public void Stop(int handle, float fadeOutTimeSeconds)
    {
        var audioSource = audioSources[handle];
        StartCoroutine(Fade(audioSource, 0.0f, fadeOutTimeSeconds));
    }
}

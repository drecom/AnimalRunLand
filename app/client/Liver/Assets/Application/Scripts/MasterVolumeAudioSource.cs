using System;
using UnityEngine;
using System.Collections;

class MasterVolumeAudioSource
{
    public AudioSource Source;
    public Transform Transform;
    public Func<float> GetMasterVolume;
    private float volume;
    public float Volume
    {
        get { return volume; }
        set
        {
            volume = value;
            Source.volume = volume * GetMasterVolume();
        }
    }
    public void UpdateVolume()
    {
        Source.volume = volume * GetMasterVolume();

        if (Source.clip != null)
        {
            if (Source.clip.name == "00_real_home" || Source.clip.name == "01_sp_home")
            {
                Source.volume = 1.0f * GetMasterVolume();
            }
        }
    }
}

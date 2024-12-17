using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource MusicSource;
    public AudioSource[] SFXSources;
    [Header("Audio Clips")]
    public AudioClips[] music;
    public AudioClips[] Clips;

    public void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void PlaySFX(AudioClips clip, bool RandomPitch)
    {
        for (int i = 0; i < SFXSources.Length; i++)
        {
            if (!SFXSources[i].isPlaying)
            {
                if (RandomPitch)
                {
                    SFXSources[i].pitch = Random.Range(1f, 2f);
                }
                else
                {
                    SFXSources[i].pitch = 1f;
                }
                SFXSources[i].clip = clip.clip;
                SFXSources[i].Play();
                break;
            }
        }
    }

    public void PlayMusic(AudioClip music)
    {
        MusicSource.Stop();
        MusicSource.clip = music;
        MusicSource.Play();
    }

}

[System.Serializable]
public struct AudioClips
{
    public string AudioName;
    public AudioClip clip;
}

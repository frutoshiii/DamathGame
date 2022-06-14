using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.loop = s.loop;
        }
    }

    private void Start()
    {
        Play("ThemeBG");
    }
    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.Name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found in the list!");
            return;
        }

        s.source.volume = s.volume;

        s.source.Play();
    }

    public void StopPlaying(string name)
    {
        
        Sound s = Array.Find(sounds, sound => sound.Name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.source.volume = s.volume * 0f;
        
        s.source.Stop();
    }

    public void MuteAudio()
    {
        AudioListener.volume = 0f;
    }
    public void UnmuteAudio()
    {
        AudioListener.volume = 1f;
    }


}

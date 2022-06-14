using UnityEngine.Audio;
using System;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public Sound[] sounds;

    public static BGMManager instance;

    private void Awake()
    {
        if (instance == null)
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

    public void MuteBGM()
    {
        AudioListener.volume = 0f;
    }

    public void UnmuteBGM()
    {
        AudioListener.volume = 1f;
    }

}

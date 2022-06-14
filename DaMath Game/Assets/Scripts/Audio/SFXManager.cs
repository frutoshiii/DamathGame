using UnityEngine.Audio;
using System;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public Sound[] sounds;

    public static SFXManager instance;

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
            s.source.mute = s.mute;
        }
    }

    private void Start()
    {
        
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
    public void MuteSFX()
    {
        /* Sound s = Array.Find(sounds, sound => sound.audioType == audioType);
         if (s == null)
         {
             Debug.LogWarning("Sound: " + audioType + " not found!");
             return;
         }*/

        AudioListener.volume = 0f;
       

        /*s.source.volume = s.volume * 0f;
        s.source.Stop();*/
    }
    
    public void UnmuteSFX()
    {
        /*Sound s = Array.Find(sounds, sound => sound.audioType == audioType);

        s.source.volume = s.volume;

        s.source.Play();*/

        AudioListener.volume = 1f;
    }
        
    
}

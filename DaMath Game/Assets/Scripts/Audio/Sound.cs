using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string Name;
    public string audioType;
 

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;


    public bool loop;
    public bool mute;
    public AudioMixerGroup group;  
    [HideInInspector]
    public AudioSource source;
}

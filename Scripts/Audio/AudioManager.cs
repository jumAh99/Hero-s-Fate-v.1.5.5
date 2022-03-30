using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    //private Sound sound;
    public static AudioManager instance;

    void Awake()
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
        //allow the music to continue when transitioning into another scene
        DontDestroyOnLoad(gameObject);
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = s.group;
        }
    }
    void Start()
    {
        //start the main menu music
        Play("MainMenu_Sound");
    }
    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound:" + name + " not found!");
            return;
        }
        s.source.Play();
    }
    public void StopPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        // s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volume / 2f, s.volume / 2f));
        // s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitch / 2f, s.pitch / 2f));
        s.source.Stop();
    }
    // public void Play(string name)
    // {
    //     StopAllCoroutines();
    //     if (sound != null) StartCoroutine(EndSound());

    //     sound = Array.Find(sounds, s => s.name == name);
    //     if (sound == null)
    //     {
    //         Debug.LogWarning("Music " + name + " not found.");
    //         return;
    //     }
    //     StartCoroutine(StartSound());
    // }

    // private IEnumerator EndSound()
    // {
    //     AudioSource oldSound = sound.source;
    //     while (oldSound.volume > 0)
    //     {
    //         oldSound.volume -= 0.01f;
    //         yield return null;
    //     }
    //     oldSound.Stop();
    // }

    // private IEnumerator StartSound()
    // {
    //     sound.source.Play();
    //     float volume = 0f;
    //     do
    //     {
    //         sound.source.volume = volume;
    //         volume += 0.01f;
    //         yield return null;
    //     } while (sound.source.volume <= sound.volume);
    // }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAudioClip : MonoBehaviour
{
    private AudioSource source;

    public List<AudioClip> Clips;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlayRandom()
    {
        if (source.isPlaying)
        {
            source.Stop();
        }

        var index = Random.Range(0, Clips.Count);
        var clip = Clips[index];

        source.clip = clip;
        source.Play();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] AudioSource bgMusicSource;

    [SerializeField] AudioClip menuMusic;
    [SerializeField] List<AudioClip> backgroundMusic;

    int currentMusicIndex = -1;
    private void Start()
    {
        SetRandomBGMusic();
        SetMenuBGMusic();
    }

    private void SetMenuBGMusic()
    {
        bgMusicSource.clip = menuMusic;
        bgMusicSource.loop = true;
        bgMusicSource.Play();
    }

    private void SetRandomBGMusic()
    {
        bgMusicSource.loop = false;

        if (backgroundMusic.Count == 0)
            return;

        if(backgroundMusic.Count != 1)
        {
            int nextMusicIndex;
            do
            {
                nextMusicIndex = Random.Range(0, backgroundMusic.Count);
            } while (nextMusicIndex == currentMusicIndex);
        }

        bgMusicSource.clip = backgroundMusic[currentMusicIndex];
        bgMusicSource.Play();
    }
}

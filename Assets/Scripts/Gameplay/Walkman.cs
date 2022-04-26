using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walkman : MonoBehaviour
{
    [SerializeField] List<AudioClip> musicList = new List<AudioClip>();
    [SerializeField] AudioSource audiosource;

    AudioClip previousSong;
    AudioClip chosenSong;
    float songLength;


    public void StopMusic()
    {
        StopCoroutine(nameof(PlaySong));
    }

    public void PlayMusic()
    {
        StartCoroutine(nameof(PlaySong));
    }

    IEnumerator PlaySong()
    {
        chosenSong = musicList[Random.Range(0, musicList.Count)];
        if (previousSong != null) while (chosenSong == previousSong) chosenSong = musicList[Random.Range(0, musicList.Count)];

        previousSong = chosenSong;
        songLength = chosenSong.length;
        audiosource.clip = chosenSong;
        audiosource.Play();
        yield return new WaitForSeconds(songLength + 3);
        StartCoroutine(nameof(PlaySong));
    }
}

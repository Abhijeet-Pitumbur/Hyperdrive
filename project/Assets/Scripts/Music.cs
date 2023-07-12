using TMPro;
using UnityEngine;

public class Music : MonoBehaviour
{

    public AudioSource audioSource;
    public AudioClip[] songs;
    public AudioClip clickSound, startSound;
    public int songIndex;
    public TextMeshProUGUI songName;
    private string songString;

    private void Start()
    {
        songIndex = 0;
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        Shuffle();
    }

    private void Shuffle()
    {
        AudioClip temp;
        int i, j;
        System.Random randomNum;
        i = songs.Length;
        j = 0;
        randomNum = new System.Random();
        while (i > 1)
        {
            i--;
            j = randomNum.Next(0, i + 1);
            temp = songs[j];
            songs[j] = songs[i];
            songs[i] = temp;
        }
    }

    private void Update()
    { if (!audioSource.isPlaying) { NextSong(); } }

    public void NextSong()
    {
        if (songIndex >= songs.Length - 1)
        { songIndex = 0; }
        else
        { songIndex++; }
        if ((Menu.ins.menuSong == songs[songIndex]) || (Menu.ins.raceSong == songs[songIndex]))
        { NextSong(); }
        PlaySong();
    }

    public void PreviousSong()
    {
        if (songIndex <= 0)
        { songIndex = songs.Length - 1; }
        else
        { songIndex--; }
        PlaySong();
    }

    private void PlaySong()
    {
        audioSource.clip = songs[songIndex];
        audioSource.Play();
        songString = audioSource.clip.ToString();
        songName.text = (songString).Substring(0, songString.Length - 24);
    }

    public void ChangeVolume(float sliderValue)
    { AudioListener.volume = Menu.ins.soundVolume = sliderValue; }

    public void PlayClickSound()
    { audioSource.PlayOneShot(clickSound); }

    public void PlayStartSound()
    { audioSource.PlayOneShot(startSound); }

}
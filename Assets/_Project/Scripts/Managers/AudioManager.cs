using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;

    [Header("Snake SFX")]
    [SerializeField] private AudioClip snakeDie;

    [Header("Food SFX")]
    [SerializeField] private AudioClip eatCommon;
    [SerializeField] private AudioClip eatRare;
    [SerializeField] private AudioClip eatEpic;

    [Header("UI SFX")]
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip buttonHover;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private int sfxPoolSize = 10;

    private List<AudioSource> sfxPool = new List<AudioSource>();

    [Header("Volume Settings")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.7f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

    [Header("Fade Settings")]
    [SerializeField] private float musicFadeDuration = 1f;

    private Dictionary<string, AudioClip> sfxLibrary = new Dictionary<string, AudioClip>();
    private Coroutine musicFadeCoroutine;
    private string currentMusicName = "";

    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;
    public float bgmVolume => musicVolume;
    public bool IsMusicPlaying => musicSource != null && musicSource.isPlaying;

    protected override void Awake()
    {
        base.Awake();
        InitializeAudioSources();
        InitializeSFXLibrary();
        LoadAudioSettings();
    }

    private void InitializeAudioSources()
    {
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
        }

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
        }

        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;

        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject poolObj = new GameObject($"SFXPool_{i}");
            poolObj.transform.SetParent(transform);
            AudioSource source = poolObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = sfxVolume;
            sfxPool.Add(source);
        }
    }

    private void InitializeSFXLibrary()
    {
        RegisterSFX("SnakeDie", snakeDie);
        RegisterSFX("Eat_Common", eatCommon);
        RegisterSFX("Eat_Rare", eatRare);
        RegisterSFX("Eat_Epic", eatEpic);
        RegisterSFX("ButtonClick", buttonClick);
        RegisterSFX("ButtonHover", buttonHover);
        RegisterSFX("EatFood", eatCommon);
    }

    private void RegisterSFX(string key, AudioClip clip)
    {
        if (clip != null && !sfxLibrary.ContainsKey(key))
        {
            sfxLibrary.Add(key, clip);
        }
    }

    private void LoadAudioSettings()
    {
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume");
        }
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            musicVolume = PlayerPrefs.GetFloat("MusicVolume");
            if (musicSource != null)
                musicSource.volume = musicVolume;
        }
        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume");
            UpdateSFXVolume();
        }
    }

    public void PlayMusic(string musicName, bool fadeIn = true)
    {
        AudioClip clip = GetMusicClip(musicName);
        if (clip == null) return;

        if (currentMusicName == musicName && musicSource.isPlaying)
            return;

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        if (fadeIn)
        {
            musicFadeCoroutine = StartCoroutine(CrossFadeMusic(clip, musicName));
        }
        else
        {
            musicSource.clip = clip;
            musicSource.Play();
            currentMusicName = musicName;
        }
    }

    public void PlayBGM(string musicName)
    {
        PlayMusic(musicName, true);
    }

    private AudioClip GetMusicClip(string name)
    {
        return name.ToLower() switch
        {
            "menu" => menuMusic,
            "gameplay" => gameplayMusic,
            _ => null
        };
    }

    private IEnumerator CrossFadeMusic(AudioClip newClip, string newName)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < musicFadeDuration / 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (musicFadeDuration / 2f));
            yield return null;
        }

        musicSource.clip = newClip;
        musicSource.Play();
        currentMusicName = newName;

        elapsed = 0f;
        while (elapsed < musicFadeDuration / 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume * masterVolume, elapsed / (musicFadeDuration / 2f));
            yield return null;
        }

        musicSource.volume = musicVolume * masterVolume;
        musicFadeCoroutine = null;
    }

    public void PlaySFX(string sfxName)
    {
        if (!sfxLibrary.ContainsKey(sfxName))
            return;

        AudioClip clip = sfxLibrary[sfxName];
        PlaySFX(clip);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        AudioSource source = GetAvailableSFXSource();
        if (source != null)
        {
            source.PlayOneShot(clip, sfxVolume * masterVolume);
        }
        else
        {
            sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
        }
    }

    private AudioSource GetAvailableSFXSource()
    {
        foreach (AudioSource source in sfxPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        return null;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
        SaveAudioSettings();
    }

    public void SetBGMVolume(float volume)
    {
        SetMusicVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateSFXVolume();
        SaveAudioSettings();
    }

    private void UpdateSFXVolume()
    {
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume * masterVolume;
        }

        foreach (AudioSource source in sfxPool)
        {
            source.volume = sfxVolume * masterVolume;
        }
    }

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }
}
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class AudioEntry
    {
        public string keyId;
        public AudioClip clip;
    }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("BGM List")]
    public List<AudioEntry> bgmList = new List<AudioEntry>();

    [Header("SFX List")]
    public List<AudioEntry> sfxList = new List<AudioEntry>();

    private Dictionary<string, AudioClip> bgmDict = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxDict = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildDictionaries();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void BuildDictionaries()
    {
        bgmDict.Clear();
        sfxDict.Clear();

        foreach (AudioEntry entry in bgmList)
        {
            if (entry == null || string.IsNullOrEmpty(entry.keyId)) continue;
            if (!bgmDict.ContainsKey(entry.keyId))
                bgmDict.Add(entry.keyId, entry.clip);
        }

        foreach (AudioEntry entry in sfxList)
        {
            if (entry == null || string.IsNullOrEmpty(entry.keyId)) continue;
            if (!sfxDict.ContainsKey(entry.keyId))
                sfxDict.Add(entry.keyId, entry.clip);
        }
    }

    public void PlayBGM(string keyId)
    {
        if (!bgmDict.ContainsKey(keyId)) return;

        AudioClip clip = bgmDict[keyId];
        if (clip == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
    }

    public void PlaySFX(string keyId)
    {
        if (!sfxDict.ContainsKey(keyId)) return;

        AudioClip clip = sfxDict[keyId];
        if (clip == null) return;

        sfxSource.PlayOneShot(clip);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    // Pool for AudioSources
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    [SerializeField] private int poolSize = 10;

    // AudioSource just for background music
    private AudioSource backgroundMusicSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize the audio source pool
            for (int i = 0; i < poolSize; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.spatialBlend = 1.0f; 
                source.rolloffMode = AudioRolloffMode.Logarithmic; 
                source.minDistance = 1.0f;
                source.maxDistance = 30.0f;
                audioSourcePool.Enqueue(source);
            }
        }
        else
        {
            Destroy(gameObject);
        }

        backgroundMusicSource = gameObject.AddComponent<AudioSource>();
        backgroundMusicSource.loop = true; 
        backgroundMusicSource.spatialBlend = 0.0f;  
        backgroundMusicSource.playOnAwake = false;
        PlayBackgroundMusic("BGM1");
    }

    public void PlaySFX(string name, Vector3 position)
    {
        foreach (Sound sound in sounds)
        {
            if (sound.name == name)
            {
                // Get an available AudioSource from the pool
                if (audioSourcePool.Count > 0)
                {
                    AudioSource source = audioSourcePool.Dequeue();
                    source.clip = sound.clip;
                    source.volume = sound.volume;
                    source.transform.position = position;
                    source.Play();

                    StartCoroutine(ReturnToPoolAfterPlayback(source));
                }
                else
                {
                    Debug.Log("No audio sources !!!!");
                }
                return;
            }
        }

    }

    public void PlayBackgroundMusic(string name)
    {
        foreach (Sound sound in sounds)
        {
            if (sound.name == name)
            {
                backgroundMusicSource.clip = sound.clip;
                backgroundMusicSource.volume = sound.volume;
                backgroundMusicSource.Play();
                return;
            }
        }
    }

    private IEnumerator ReturnToPoolAfterPlayback(AudioSource source)
    {
        yield return new WaitForSeconds(source.clip.length);
        source.Stop();
        source.clip = null;
        audioSourcePool.Enqueue(source);
    }
}

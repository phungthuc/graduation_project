using System.Collections.Generic;
using UnityEngine;

namespace TheTunnel.Manager
{
    public class GameSoundManager : MonoBehaviour
    {
        private static GameSoundManager instance;

        public static GameSoundManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<GameSoundManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("GameSoundManager");
                        instance = go.AddComponent<GameSoundManager>();
                    }
                }
                return instance;
            }
        }

        private const int MAX_AUDIO_SOURCES = 5;
        private Dictionary<string, List<AudioSource>> _audioSourceDict = new();

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlaySound(AudioClip clip, float pitchAdded, bool randomPitch, float spatialBlend, Vector3 position)
        {
            if (clip == null)
            {
                return;
            }
            AudioSource audioSource = GetAudioSource(clip.name);
            if (audioSource == null)
            {
                return;
            }
            audioSource.spatialBlend = spatialBlend;
            float pitch = randomPitch ? Random.Range(-pitchAdded, pitchAdded) : pitchAdded;
            audioSource.pitch = 1 + pitch;
            if (spatialBlend > 0)
            {
                audioSource.transform.position = position;
            }
            audioSource.PlayOneShot(clip);
        }

        private AudioSource GetAudioSource(string id)
        {
            if (!_audioSourceDict.ContainsKey(id))
            {
                _audioSourceDict.Add(id, new List<AudioSource>());
                return CreateAudioSource(id);
            }
            foreach (AudioSource audioSource in _audioSourceDict[id])
            {
                if (!audioSource.isPlaying)
                {
                    return audioSource;
                }
            }
            // Limit the number of audio sources
            if (_audioSourceDict[id].Count < MAX_AUDIO_SOURCES)
            {
                return CreateAudioSource(id);
            }
            return null;
        }

        private AudioSource CreateAudioSource(string id)
        {
            GameObject go = new GameObject(id);
            go.transform.parent = transform;
            AudioSource audioSource = go.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            _audioSourceDict[id].Add(audioSource);
            return audioSource;
        }
    }
}

using UnityEngine;
using System.Collections;

namespace cowsins
{
    /// <summary>
    /// Global 2D Sound Manager (UI, weapon SFX, v.v.)
    /// Được giữ lại khi chuyển scene để tránh MissingReference sau khi Netcode load scene mới.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;

        private AudioSource src;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.parent = null;
                DontDestroyOnLoad(gameObject); // Giữ lại qua các scene

                src = GetComponent<AudioSource>();
                if (src == null)
                {
                    src = gameObject.AddComponent<AudioSource>();
                }
            }
            else if (Instance != this)
            {
                // Nếu đã có instance khác, destroy object mới
                Destroy(gameObject);
            }
        }

        public void PlaySound(AudioClip clip, float delay, float pitchAdded, bool randomPitch, float spatialBlend)
        {
            // Nếu không có instance hoặc clip null thì bỏ qua để tránh lỗi
            if (Instance == null || clip == null)
                return;

            StartCoroutine(Play(clip, delay, pitchAdded, randomPitch, spatialBlend));
        }

        private IEnumerator Play(AudioClip clip, float delay, float pitch, bool randomPitch, float spatialBlend)
        {
            if (clip == null)
                yield break;

            // src có thể null nếu AudioSource bị remove
            if (src == null)
            {
                src = GetComponent<AudioSource>();
                if (src == null)
                    yield break;
            }

            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            src.spatialBlend = spatialBlend;
            float pitchAdded = randomPitch ? Random.Range(-pitch, pitch) : pitch;
            src.pitch = 1 + pitchAdded;
            src.PlayOneShot(clip);
        }
    }
}



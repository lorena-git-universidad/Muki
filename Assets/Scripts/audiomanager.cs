using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [System.Serializable]
        public class Sound
        {
            public string soundName;
            public AudioClip clip;

            [Range(0f, 1f)]
            public float volume = 1f;

            public bool loop;
            public bool playOnAwake;
        }

        [Header("Sounds")]
        public Sound[] sounds;

        private Dictionary<string, Sound> soundDictionary;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);

            CreateSoundDictionary();
        }

        private void CreateSoundDictionary()
        {
            soundDictionary = new Dictionary<string, Sound>();

            if (sounds == null)
                return;

            foreach (Sound sound in sounds)
            {
                if (sound == null ||
                    string.IsNullOrEmpty(sound.soundName))
                    continue;

                if (sound.clip == null)
                    continue;

                soundDictionary[sound.soundName] = sound;
            }
        }

        private bool TryGetSound(string soundName, out Sound sound)
        {
            sound = null;

            if (soundDictionary == null ||
                string.IsNullOrEmpty(soundName) ||
                !soundDictionary.TryGetValue(soundName, out sound))
            {
                Debug.LogWarning("No existe el sonido: " + soundName);
                return false;
            }

            return true;
        }

        // ==========================================
        // PLAY SOUND (ahora devuelve el AudioSource)
        // ==========================================

        public AudioSource PlaySound(string soundName)
        {
            if (!TryGetSound(soundName, out Sound sound))
                return null;

            GameObject soundObject =
                new GameObject("Sound_" + soundName);

            soundObject.transform.SetParent(transform);

            AudioSource source =
                soundObject.AddComponent<AudioSource>();

            source.clip = sound.clip;
            source.volume = sound.volume;
            source.loop = sound.loop;

            source.Play();

            if (!sound.loop)
            {
                Destroy(soundObject, sound.clip.length);
            }

            return source;
        }

        // ==========================================
        // LOOP CONTROLABLE
        // ==========================================
        // Devuelve el AudioSource para poder cambiar su pitch,
        // pausarlo o detenerlo con StopLoop.
        // Si followTarget no es null, el sonido sigue a ese objeto.

        public AudioSource PlayLoop(
            string soundName,
            Transform followTarget = null,
            float spatialBlend = 0f,
            float minDistance = 2f,
            float maxDistance = 25f)
        {
            if (!TryGetSound(soundName, out Sound sound))
                return null;

            GameObject loopObject =
                new GameObject("Loop_" + soundName);

            loopObject.transform.SetParent(
                followTarget != null ? followTarget : transform
            );

            loopObject.transform.localPosition = Vector3.zero;

            AudioSource source =
                loopObject.AddComponent<AudioSource>();

            source.clip = sound.clip;
            source.volume = sound.volume;
            source.loop = true;
            source.spatialBlend = spatialBlend;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.rolloffMode = AudioRolloffMode.Linear;

            source.Play();

            return source;
        }

        public void StopLoop(AudioSource source, float fadeTime = 0f)
        {
            if (source == null)
                return;

            if (fadeTime <= 0f)
            {
                Destroy(source.gameObject);
                return;
            }

            StartCoroutine(FadeOutAndDestroy(source, fadeTime));
        }

        private IEnumerator FadeOutAndDestroy(
            AudioSource source,
            float fadeTime)
        {
            float startVolume = source.volume;
            float elapsed = 0f;

            while (source != null && elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                source.volume =
                    Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);

                yield return null;
            }

            if (source != null)
                Destroy(source.gameObject);
        }

        // ==========================================
        // NOISE
        // ==========================================

        public void EmitNoise(
            Vector3 position,
            float radius,
            string noiseType)
        {
            Debug.Log(
                $"<color=yellow>RUIDO:</color> " +
                $"{noiseType} | Radio: {radius}"
            );

            NotifyEnemies(position, radius, noiseType);
        }

        private void NotifyEnemies(
            Vector3 position,
            float radius,
            string noiseType)
        {
            MukiEnemy[] enemies =
                FindObjectsByType<MukiEnemy>(
                    FindObjectsSortMode.None
                );

            foreach (MukiEnemy enemy in enemies)
            {
                if (enemy != null)
                {
                    enemy.HearNoise(position, radius, noiseType);
                }
            }
        }
    }
}
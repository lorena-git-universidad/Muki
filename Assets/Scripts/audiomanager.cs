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
            soundDictionary =
                new Dictionary<string, Sound>();

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

        // ==========================================
        // PLAY SOUND
        // ==========================================

        public void PlaySound(string soundName)
        {
            if (!soundDictionary.ContainsKey(soundName))
            {
                Debug.LogWarning(
                    "No existe el sonido: " +
                    soundName
                );

                return;
            }

            Sound sound =
                soundDictionary[soundName];

            GameObject soundObject =
                new GameObject(
                    "Sound_" + soundName
                );

            soundObject.transform.SetParent(transform);

            AudioSource source =
                soundObject.AddComponent<AudioSource>();

            source.clip = sound.clip;
            source.volume = sound.volume;
            source.loop = sound.loop;

            source.Play();

            if (!sound.loop)
            {
                Destroy(
                    soundObject,
                    sound.clip.length
                );
            }
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

            NotifyEnemies(
                position,
                radius,
                noiseType
            );
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
                    enemy.HearNoise(
                        position,
                        radius,
                        noiseType
                    );
                }
            }
        }
    }
}
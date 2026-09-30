
using UnityEngine;

namespace StarterAssets
{
    public class RadioInteractable : MonoBehaviour
    {
        [Header("Audio")]
        public AudioClip radioAudio;

        [Range(0f, 1f)]
        public float volume = 1f;

        [Header("Animations")]
        public Animator radioAnimator;
        public string playingAnimation = "radioSonando";
        public string idleAnimation = "radioIdle";

        private AudioSource audioSource;
        private bool hasStarted = false;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = volume;
            audioSource.clip = radioAudio;
        }

        private void Update()
        {
            // Si el audio terminó, regresar a la animación idle.
            if (hasStarted &&
                !audioSource.isPlaying &&
                audioSource.time >= audioSource.clip.length)
            {
                hasStarted = false;
                PlayAnimation(idleAnimation);
            }
        }

        public void Interact()
        {
            if (radioAudio == null)
            {
                Debug.LogWarning(
                    "Esta radio no tiene un audio asignado: " + name
                );

                return;
            }

            // Si el audio está reproduciéndose, pausarlo.
            if (audioSource.isPlaying)
            {
                audioSource.Pause();

                Debug.Log("Radio pausada: " + name);
                return;
            }

            // Si ya se había iniciado, continuar desde la pausa.
            if (hasStarted)
            {
                audioSource.UnPause();

                Debug.Log("Radio reanudada: " + name);
                return;
            }

            // Primera reproducción o reproducción después de terminar.
            audioSource.clip = radioAudio;
            audioSource.volume = volume;
            audioSource.time = 0f;
            audioSource.Play();

            hasStarted = true;

            PlayAnimation(playingAnimation);

            Debug.Log("Reproduciendo radio: " + name);
        }

        private void PlayAnimation(string animationName)
        {
            if (radioAnimator == null ||
                string.IsNullOrEmpty(animationName))
                return;

            radioAnimator.Play(animationName, 0, 0f);
        }
    }
}

using UnityEngine;

namespace StarterAssets
{
    public class RadioInteractable : MonoBehaviour
    {
        [Header("Audio")]
        public AudioClip radioAudio;

        [Range(0f, 1f)]
        public float volume = 1f;

        [Header("Distancia del jugador")]
        public Transform player;
        public float maxDistance = 7f;

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
            // Si la radio está sonando, comprobar la distancia.
            if (audioSource.isPlaying && player != null)
            {
                float distance = Vector3.Distance(
                    transform.position,
                    player.position
                );

                if (distance >= maxDistance)
                {
                    StopRadio();
                    Debug.Log("Radio detenida: el jugador se alejó.");
                    return;
                }
            }

            // Si el audio terminó, regresar a la animación idle.
            if (hasStarted &&
                !audioSource.isPlaying &&
                audioSource.clip != null &&
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

            // Evitar iniciar la radio si el jugador está muy lejos.
            if (player != null &&
                Vector3.Distance(transform.position, player.position)
                    >= maxDistance)
            {
                Debug.Log("Estás demasiado lejos de la radio.");
                return;
            }

            // Si está reproduciéndose, pausar.
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
                Debug.Log("Radio pausada: " + name);
                return;
            }

            // Si estaba pausada, continuar.
            if (hasStarted && audioSource.time > 0f)
            {
                audioSource.UnPause();
                PlayAnimation(playingAnimation);
                Debug.Log("Radio reanudada: " + name);
                return;
            }

            // Primera reproducción o reproducción desde el inicio.
            audioSource.clip = radioAudio;
            audioSource.volume = volume;
            audioSource.time = 0f;
            audioSource.Play();

            hasStarted = true;
            PlayAnimation(playingAnimation);

            Debug.Log("Reproduciendo radio: " + name);
        }

        // Llama a este método cuando el jugador muera.
        public void StopRadio()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.time = 0f;
            }

            hasStarted = false;
            PlayAnimation(idleAnimation);
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

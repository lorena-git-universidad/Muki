using System.Collections.Generic;
using UnityEngine;

namespace Muki.UI
{
    /// <summary>
    /// Añadir al mismo objeto que RadioInteractable. Detecta cuando la noticia
    /// termina de sonar (no cuando se pausa) y lanza los pensamientos de esa radio.
    /// </summary>
    [RequireComponent(typeof(StarterAssets.RadioInteractable))]
    public sealed class RadioThoughtTrigger : MonoBehaviour
    {
        [SerializeField] private List<Thought> thoughts = new List<Thought>();
        [SerializeField] private bool onlyOnce = true;

        private AudioSource source;
        private float lastTime;
        private bool wasPlaying;
        private bool done;

        private void Update()
        {
            if (done && onlyOnce) return;

            // RadioInteractable crea el AudioSource en su Awake; se busca aquí por si aún no existía.
            if (source == null) source = GetComponent<AudioSource>();
            if (source == null || source.clip == null) return;

            if (source.isPlaying)
            {
                wasPlaying = true;
                lastTime = source.time;
                return;
            }

            if (!wasPlaying) return;
            wasPlaying = false;

            // Pausa: el tiempo queda lejos del final. Fin real: estaba en el último instante.
            bool finished = lastTime >= source.clip.length - 0.3f;
            if (!finished) return;

            done = true;
            if (ThoughtDialogUI.Instance != null)
                ThoughtDialogUI.Instance.Show(thoughts);
            else
                Debug.LogWarning("RadioThoughtTrigger: no hay ThoughtDialogUI en la escena.");
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace Muki.UI
{
    /// <summary>
    /// Muestra a la derecha el mapa de Latinoamérica. Por defecto el mapa normal; cuando el cursor
    /// (o la selección con teclado) está sobre un botón de leyenda, cambia por el mapa resaltado
    /// de esa leyenda con un fundido cruzado.
    /// </summary>
    public sealed class LevelMapHover : MonoBehaviour
    {
        [Header("Imágenes (la de arriba hace el fundido)")]
        [SerializeField] private RawImage front;
        [SerializeField] private RawImage back;

        [Header("Mapa por defecto")]
        [SerializeField] private Texture normalMap;

        [Header("Animación")]
        [SerializeField] private float fadeDuration = 0.25f;

        private Texture target;
        private float fade = 1f;

        private void Awake()
        {
            target = normalMap;
            if (front != null) { front.texture = normalMap; front.color = Color.white; }
            if (back != null) { back.texture = normalMap; back.color = Color.white; }
        }

        private void Update()
        {
            if (front == null || fade >= 1f) return;

            fade = Mathf.Min(1f, fade + Time.unscaledDeltaTime / Mathf.Max(0.01f, fadeDuration));
            front.color = new Color(1f, 1f, 1f, Mathf.SmoothStep(0f, 1f, fade));
        }

        public void Hover(Texture map) => SetMap(map != null ? map : normalMap);

        /// <summary>Vuelve al mapa normal, pero solo si el mapa actual es el de ese botón.</summary>
        public void Unhover(Texture map)
        {
            if (target == map) SetMap(normalMap);
        }

        private void SetMap(Texture map)
        {
            if (map == target || front == null) return;

            // El mapa que se veía pasa al fondo y el nuevo aparece encima.
            if (back != null)
            {
                back.texture = front.texture;
                back.color = Color.white;
            }

            target = map;
            front.texture = map;
            front.color = new Color(1f, 1f, 1f, 0f);
            fade = 0f;
        }
    }
}

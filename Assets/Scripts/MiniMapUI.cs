using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class MiniMapUI : MonoBehaviour
    {
        [Header("UI")]
        public GameObject miniMapPanel;

        private void Start()
        {
            if (miniMapPanel != null)
                miniMapPanel.SetActive(false);
        }

        private void Update()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                ToggleMiniMap();
            }
        }

        private void ToggleMiniMap()
        {
            if (miniMapPanel == null)
                return;

            bool newState = !miniMapPanel.activeSelf;

            miniMapPanel.SetActive(newState);
        }
    }
}
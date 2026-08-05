using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class PlayerHide : MonoBehaviour
    {
        [Header("References")]
        public CharacterController controller;
        public FirstPersonController movement;

        public bool IsHidden { get; private set; }

        private Transform currentHidePoint;

        private Vector3 exitPosition;
        private Quaternion exitRotation;

        private int enterFrame;

        private void Awake()
        {
            if (controller == null)
                controller = GetComponent<CharacterController>();

            if (movement == null)
                movement = GetComponent<FirstPersonController>();
        }

        private void Update()
        {
            if (!IsHidden)
                return;

            // Evita salir el mismo frame en el que entró
            if (Time.frameCount <= enterFrame)
                return;

            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                ExitHide();
            }
        }

        public void ToggleHide(Transform hidePoint)
        {
            if (IsHidden)
            {
                ExitHide();
            }
            else
            {
                EnterHide(hidePoint);
            }
        }

        private void EnterHide(Transform hidePoint)
        {
            currentHidePoint = hidePoint;

            exitPosition = transform.position;
            exitRotation = transform.rotation;

            IsHidden = true;

            enterFrame = Time.frameCount;

            // Desactivar el controller para mover libremente
            controller.enabled = false;

            // Mover exactamente al HidePoint
            transform.SetPositionAndRotation(
                hidePoint.position,
                hidePoint.rotation);

            // Desactivar movimiento
            movement.enabled = false;
        }

        private void ExitHide()
        {
            IsHidden = false;

            // Volver al sitio donde estaba
            transform.SetPositionAndRotation(
                exitPosition,
                exitRotation);

            controller.enabled = true;

            movement.enabled = true;

            currentHidePoint = null;
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class PlayerHide : MonoBehaviour
    {
        public bool IsHidden;

        public MonoBehaviour movementScript;

        private CharacterController controller;

        private Vector3 exitPosition;

        private Transform currentHidePoint;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            if (IsHidden && Keyboard.current.eKey.wasPressedThisFrame)
            {
                ExitHide();
            }
        }
        public void ToggleHide(Transform hidePoint)
        {
            Debug.Log("ToggleHide llamado");
            if (!IsHidden)
            {
                EnterHide(hidePoint);
            }
            
        }

        void EnterHide(Transform hidePoint)
        {
            Debug.Log("Entrando al escondite");
            IsHidden = true;

            currentHidePoint = hidePoint;

            exitPosition = transform.position;

            controller.enabled = false;

            transform.position = hidePoint.position;
            transform.rotation = hidePoint.rotation;

            controller.enabled = true;

            movementScript.enabled = false;
        }

        void ExitHide()
        {
            IsHidden = false;

            controller.enabled = false;

            transform.position = exitPosition;

            controller.enabled = true;

            movementScript.enabled = true;
        }
    }
}
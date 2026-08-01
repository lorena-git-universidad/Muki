using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCrouch : MonoBehaviour
    {
        [Header("References")]
        public StarterAssetsInputs input;
        public CharacterController controller;
        public Transform cameraTarget;

        [Header("Crouch Settings")]
        public float standingHeight = 2f;
        public float crouchingHeight = 1f;

        public float standingCameraY = 0.9f;
        public float crouchingCameraY = 0.45f;

        public float crouchSpeed = 8f;

        private void Awake()
        {
            if (controller == null)
                controller = GetComponent<CharacterController>();

            if (input == null)
                input = GetComponent<StarterAssetsInputs>();
        }
        
        void Update()
        {
            Debug.Log(input.crouch);
            bool isCrouching = UnityEngine.InputSystem.Keyboard.current.cKey.isPressed;

            float targetHeight = isCrouching ? crouchingHeight : standingHeight;
            float targetY = isCrouching ? crouchingCameraY : standingCameraY; 
            controller.height = Mathf.Lerp(
                controller.height,
                targetHeight,
                Time.deltaTime * crouchSpeed);

            controller.center = new Vector3(
                0,
                controller.height / 2f,
                0);

            if (cameraTarget != null)
            {
                Vector3 pos = cameraTarget.localPosition;

                    pos.y = Mathf.Lerp(
                    pos.y,
                    targetY,
                    Time.deltaTime * crouchSpeed);

                cameraTarget.localPosition = pos;
            }
        }
    }
}
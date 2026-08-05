using UnityEngine;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCrouch : MonoBehaviour
    {
        [Header("References")]
        public CharacterController controller;
        public StarterAssetsInputs input;
        public Transform cameraTarget;

        [Header("Heights")]
        public float standingHeight = 2f;
        public float crouchingHeight = 1f;

        [Header("Camera")]
        public float standingCameraY = 0.9f;
        public float crouchingCameraY = 0.45f;

        [Header("Smooth")]
        public float crouchSpeed = 10f;

        private Vector3 originalCenter;
        private float currentVelocityHeight;
        private float currentVelocityCamera;

        public bool IsCrouching => input != null && input.crouch;

        private void Awake()
        {
            if (controller == null)
                controller = GetComponent<CharacterController>();

            if (input == null)
                input = GetComponent<StarterAssetsInputs>();

            originalCenter = controller.center;
        }

        private void Update()
        {
            if (controller == null || input == null)
                return;

            Debug.Log(input.crouch);

            float targetHeight = input.crouch ? crouchingHeight : standingHeight;

            controller.height = Mathf.SmoothDamp(
                controller.height,
                targetHeight,
                ref currentVelocityHeight,
                1f / crouchSpeed);

            controller.center = new Vector3(
                originalCenter.x,
                originalCenter.y - ((standingHeight - controller.height) * 0.5f),
                originalCenter.z);

            if (cameraTarget != null)
            {
                float targetCameraY = input.crouch
                    ? crouchingCameraY
                    : standingCameraY;

                Vector3 pos = cameraTarget.localPosition;

                pos.y = Mathf.SmoothDamp(
                    pos.y,
                    targetCameraY,
                    ref currentVelocityCamera,
                    1f / crouchSpeed);

                cameraTarget.localPosition = pos;
            }
        }
    }
}
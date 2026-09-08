using UnityEngine;

namespace StarterAssets
{
    public class FirstPersonCameraEffects : MonoBehaviour
    {
        [Header("References")]
        public FirstPersonController player;
        public StarterAssetsInputs input;
        public PlayerCrouch crouch;

        [Header("Walking Bob")]
        public float walkBobSpeed = 8f;
        public float walkBobAmount = 0.20f;

        [Header("Running Bob")]
        public float sprintBobSpeed = 12f;
        public float sprintBobAmount = 0.25f;

        [Header("Side Movement")]
        public float sideBobAmount = 0.025f;

        [Header("Smoothness")]
        public float positionSmooth = 12f;

        [Header("Sprint FOV")]
        public float normalFOV = 70f;
        public float sprintFOV = 78f;
        public float fovSmoothSpeed = 6f;

        private Camera cam;

        private float bobTimer;

        // Posición que tenía la cámara antes de aplicar el bob
        private float baseCameraY;

        private void Awake()
        {
            if (player == null)
                player = GetComponentInParent<FirstPersonController>();

            if (player != null)
            {
                if (input == null)
                    input = player.GetComponent<StarterAssetsInputs>();

                if (crouch == null)
                    crouch = player.GetComponent<PlayerCrouch>();
            }

            cam = Camera.main;

            if (cam != null)
                normalFOV = cam.fieldOfView;
        }

        private void LateUpdate()
        {
            if (player == null)
                return;

            HandleHeadBob();
            HandleSprintFOV();
        }

        private void HandleHeadBob()
        {
            if (input == null)
                return;

            // ==========================================
            // POSICIÓN BASE ACTUAL
            // ==========================================
            //
            // IMPORTANTE:
            // PlayerCrouch ya modificó la posición
            // antes de llegar aquí.
            //
            // Guardamos esa posición como nuestra base.

            baseCameraY = transform.localPosition.y;

            bool isMoving =
                input.move.sqrMagnitude > 0.01f;

            bool isSprinting =
                input.sprint &&
                !IsCrouching();

            // ==========================================
            // JUGADOR QUIETO
            // ==========================================

            if (!isMoving)
            {
                bobTimer = 0f;

                // Quitamos solamente el efecto del bob.
                // NO modificamos la posición que puso crouch.

                Vector3 position = transform.localPosition;

                position.y = baseCameraY;
                position.x = transform.localPosition.x;

                transform.localPosition = position;

                return;
            }

            // ==========================================
            // JUGADOR MOVIÉNDOSE
            // ==========================================

            float bobSpeed;
            float bobAmount;

            if (isSprinting)
            {
                bobSpeed = sprintBobSpeed;
                bobAmount = sprintBobAmount;
            }
            else
            {
                bobSpeed = walkBobSpeed;
                bobAmount = walkBobAmount;
            }

            bobTimer += Time.deltaTime * bobSpeed;

            // Movimiento vertical
            float vertical =
                Mathf.Sin(bobTimer) * bobAmount;

            // Movimiento lateral
            float horizontal =
                Mathf.Cos(bobTimer * 0.5f) *
                sideBobAmount;

            Vector3 targetPosition =
                transform.localPosition;

            targetPosition.x += horizontal;
            targetPosition.y += vertical;

            transform.localPosition =
                Vector3.Lerp(
                    transform.localPosition,
                    targetPosition,
                    Time.deltaTime * positionSmooth
                );
        }

        private void HandleSprintFOV()
        {
            if (cam == null)
                cam = Camera.main;

            if (cam == null)
                return;

            bool isSprinting =
                input != null &&
                input.sprint &&
                !IsCrouching();

            float targetFOV =
                isSprinting
                    ? sprintFOV
                    : normalFOV;

            cam.fieldOfView =
                Mathf.Lerp(
                    cam.fieldOfView,
                    targetFOV,
                    Time.deltaTime * fovSmoothSpeed
                );
        }

        private bool IsCrouching()
        {
            return crouch != null &&
                   crouch.IsCrouching;
        }
    }
}
using UnityEngine;

namespace StarterAssets
{
    public class PlayerNoise : MonoBehaviour
    {
        [Header("References")]
        public FirstPersonController movement;
        public PlayerCrouch crouch;
        public PlayerMining mining;

        [Header("Noise Radius")]
        public float crouchNoise = 2f;
        public float walkNoise = 3f;
        public float sprintNoise = 10f;
        public float miningNoise = 12f;

        [Header("Noise Frequency")]
        public float noiseInterval = 0.5f;

        [Header("Debug")]
        public bool showDebug = true;

        [Header("Mining Audio")]
        public float miningSoundInterval = 0.6f;

        private float miningSoundTimer;

        [Header("Footstep Audio")]
        public float footstepIntervalWalk = 0.5f;
        public float footstepIntervalSprint = 0.3f;
        public float footstepIntervalCrouch = 0.7f;

        private float footstepTimer;

        private string currentNoiseType;
        private float currentNoiseRadius;

        private float noiseTimer;

        private void Awake()
        {
            if (movement == null)
                movement = GetComponent<FirstPersonController>();

            if (crouch == null)
                crouch = GetComponent<PlayerCrouch>();

            if (mining == null)
                mining = GetComponent<PlayerMining>();
        }

        private void Update()
        {
            noiseTimer -= Time.deltaTime;
            footstepTimer -= Time.deltaTime;
            miningSoundTimer -= Time.deltaTime;

            HandleMiningAudio();

            float noiseRadius = GetCurrentNoiseRadius();

            // ==========================================
            // RUIDO PARA EL MUKI
            // ==========================================

            if (noiseTimer <= 0f && noiseRadius > 0f)
            {
                EmitNoise(noiseRadius);

                noiseTimer = noiseInterval;
            }

            // ==========================================
            // SONIDOS DE PASOS
            // ==========================================

            HandleFootsteps();
        }

        private float GetCurrentNoiseRadius()
        {
            // ==========================================
            // PICANDO
            // ==========================================

            if (mining != null && mining.IsMining)
            {
                return miningNoise;
            }

            // ==========================================
            // SIN MOVIMIENTO
            // ==========================================

            if (movement == null)
                return 0f;

            Vector3 velocity =
                movement.GetComponent<CharacterController>().velocity;

            float horizontalSpeed =
                new Vector3(
                    velocity.x,
                    0f,
                    velocity.z
                ).magnitude;

            if (horizontalSpeed < 0.1f)
                return 0f;

            // ==========================================
            // AGACHADO
            // ==========================================

            if (crouch != null && crouch.IsCrouching)
            {
                return crouchNoise;
            }

            // ==========================================
            // CORRIENDO
            // ==========================================

            if (movement.GetComponent<StarterAssetsInputs>().sprint)
            {
                return sprintNoise;
            }

            // ==========================================
            // CAMINANDO
            // ==========================================

            return walkNoise;
        }

        private void HandleFootsteps()
        {
            if (movement == null)
                return;

            CharacterController controller =
                movement.GetComponent<CharacterController>();

            if (controller == null)
                return;

            Vector3 velocity = controller.velocity;

            float horizontalSpeed =
                new Vector3(
                    velocity.x,
                    0f,
                    velocity.z
                ).magnitude;

            // No está caminando
            if (horizontalSpeed < 0.1f)
            {
                footstepTimer = 0f;
                return;
            }

            // ==========================================
            // AGACHADO
            // ==========================================

            if (crouch != null && crouch.IsCrouching)
            {
                if (footstepTimer <= 0f)
                {
                    PlayFootstep("PasosAgachado");

                    footstepTimer = footstepIntervalCrouch;
                }

                return;
            }

            // ==========================================
            // CORRIENDO
            // ==========================================

            StarterAssetsInputs input =
                movement.GetComponent<StarterAssetsInputs>();

            if (input != null && input.sprint)
            {
                if (footstepTimer <= 0f)
                {
                    PlayFootstep("PasosCorrer");

                    footstepTimer = footstepIntervalSprint;
                }

                return;
            }

            // ==========================================
            // CAMINANDO
            // ==========================================

            if (footstepTimer <= 0f)
            {
                PlayFootstep("Pasos");

                footstepTimer = footstepIntervalWalk;
            }
        }

        private void HandleMiningAudio()
        {
            if (mining == null || !mining.IsMining)
            {
                miningSoundTimer = 0f;
                return;
            }

            if (miningSoundTimer <= 0f)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound("Picar");
                }

                miningSoundTimer = miningSoundInterval;
            }
        }

        private void PlayFootstep(string soundName)
        {
            if (AudioManager.Instance == null)
                return;

            AudioManager.Instance.PlaySound(soundName);
        }

        private void EmitNoise(float radius)
        {
            if (AudioManager.Instance == null)
                return;

            string noiseType = GetNoiseType();

            currentNoiseType = noiseType;
            currentNoiseRadius = radius;

            if (showDebug)
            {
                Debug.Log(
                    $"<color=cyan>PLAYER NOISE:</color> " +
                    $"{noiseType} | Radio: {radius}"
                );
            }

            AudioManager.Instance.EmitNoise(
                transform.position,
                radius,
                noiseType
            );
        }

        private string GetNoiseType()
        {
            if (mining != null && mining.IsMining)
                return "Picar";

            if (crouch != null && crouch.IsCrouching)
                return "Agachado";

            if (movement != null)
            {
                StarterAssetsInputs input =
                    movement.GetComponent<StarterAssetsInputs>();

                if (input != null && input.sprint)
                    return "Correr";
            }

            return "Caminar";
        }

        private void OnDrawGizmosSelected()
        {
            if (!showDebug)
                return;

            if (currentNoiseRadius <= 0f)
                return;

            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(
                transform.position,
                currentNoiseRadius
            );
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class PlayerMining : MonoBehaviour
    {
        [Header("References")]
        public Camera playerCamera;
        public Transform pickaxeHolder;

        [Tooltip("Animator del personaje minero, no el del pico.")]
        public Animator playerAnimator;

        [Header("Interaction")]
        public float interactionDistance = 3.5f;
        public LayerMask mineableLayer;

        [Header("Pickaxe Position")]
        public Vector3 equippedPosition =
            new Vector3(0.35f, -0.35f, 0.60f);

        public Vector3 equippedRotation =
            new Vector3(15f, -80f, 20f);

        [Header("Animation")]
        public float swingSpeed = 12f;
        public float swingAngle = 35f;

        [Tooltip("Nombre exacto del estado de picar en el Animator.")]
        public string miningAnimationName = "picar_manoDer";

        [Header("State")]
        public bool hasPickaxe;

        [Header("Mining UI")]
        public MiningProgressUI miningProgressUI;

        [HideInInspector]
        public GameObject equippedPickaxe;

        private Quaternion baseRotation;
        private Vector3 basePosition;

        private float swingTimer;
        private bool isMining;

        public bool IsMining => isMining;

        private MineableRock currentRock;

        private void Awake()
        {
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();

                if (playerCamera == null)
                    playerCamera = Camera.main;
            }

            if (pickaxeHolder == null && playerCamera != null)
            {
                pickaxeHolder =
                    playerCamera.transform.Find("PickaxeHolder");
            }

            if (playerAnimator == null)
            {
                playerAnimator = GetComponentInChildren<Animator>();
            }

            if (miningProgressUI == null)
            {
                miningProgressUI =
                    FindFirstObjectByType<MiningProgressUI>();
            }
        }

        private void Update()
        {
            if (!hasPickaxe)
            {
                StopMining();
                return;
            }

            CheckMining();
        }

        private void CheckMining()
        {
            if (playerCamera == null)
            {
                StopMining();
                return;
            }

            Ray ray = new Ray(
                playerCamera.transform.position,
                playerCamera.transform.forward
            );

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    interactionDistance,
                    mineableLayer))
            {
                StopMining();
                return;
            }

            MineableRock rock =
                hit.collider.GetComponentInParent<MineableRock>();

            if (rock == null)
            {
                StopMining();
                return;
            }

            if (Keyboard.current == null ||
                !Keyboard.current.eKey.isPressed)
            {
                StopMining();
                return;
            }

            currentRock = rock;

            if (!isMining)
            {
                isMining = true;
                PlayMiningAnimation();
            }

            AnimatePickaxe();

            bool finished =
                currentRock.Mine(Time.deltaTime);

            if (miningProgressUI != null)
            {
                miningProgressUI.ShowProgress(
                    currentRock.MiningProgress
                );
            }

            if (finished)
            {
                StopMining();
            }
        }

        private void PlayMiningAnimation()
        {
            if (playerAnimator == null)
                return;

            playerAnimator.Play(
                miningAnimationName,
                0,
                0f
            );
        }

        private void StopMining()
        {
            bool wasMining = isMining;

            isMining = false;

            if (wasMining && playerAnimator != null)
            {
                // Regresa al estado por defecto del Animator,
                // que debe ser el idle del personaje.
                playerAnimator.Play("picomano_manoDer", 0, 0f);
            }

            if (miningProgressUI != null)
            {
                miningProgressUI.HideProgress();
            }

            if (currentRock != null)
            {
                currentRock.ResetMiningProgress();
                currentRock = null;
            }

            ResetPickaxe();
        }

        public bool TryEquipPickaxe(GameObject pickaxeObject)
        {
            if (pickaxeObject == null)
                return false;

            if (pickaxeHolder == null)
            {
                Debug.LogError(
                    "No se encontró PickaxeHolder."
                );

                return false;
            }

            equippedPickaxe = pickaxeObject;
            hasPickaxe = true;

            // Desactivar la física del pico.
            Collider[] cols =
                equippedPickaxe.GetComponentsInChildren<Collider>();

            foreach (Collider c in cols)
            {
                c.enabled = false;
            }

            Rigidbody rb =
                equippedPickaxe.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // Ignorar raycasts con el pico equipado.
            int ignoreRaycastLayer =
                LayerMask.NameToLayer("Ignore Raycast");

            if (ignoreRaycastLayer != -1)
            {
                equippedPickaxe.layer = ignoreRaycastLayer;

                foreach (Transform child in
                         equippedPickaxe.GetComponentsInChildren<Transform>())
                {
                    child.gameObject.layer = ignoreRaycastLayer;
                }
            }

            // Colocar el pico en la cámara.
            equippedPickaxe.transform.SetParent(pickaxeHolder);

            equippedPickaxe.transform.localPosition =
                equippedPosition;

            equippedPickaxe.transform.localRotation =
                Quaternion.Euler(equippedRotation);

            basePosition =
                equippedPickaxe.transform.localPosition;

            baseRotation =
                equippedPickaxe.transform.localRotation;

            // Ocultar el sprite del pico físico.
            SetPickaxeVisible(false);

            Debug.Log("Pico equipado. Pico físico oculto.");

            return true;
        }

        public void SetPickaxeVisible(bool visible)
        {
            if (equippedPickaxe == null)
                return;

            SpriteRenderer[] sprites =
                equippedPickaxe.GetComponentsInChildren<SpriteRenderer>();

            foreach (SpriteRenderer sprite in sprites)
            {
                sprite.enabled = visible;
            }
        }

        private void AnimatePickaxe()
        {
            if (equippedPickaxe == null)
                return;

            swingTimer += Time.deltaTime * swingSpeed;

            float angle =
                Mathf.Sin(swingTimer) * swingAngle;

            equippedPickaxe.transform.localRotation =
                baseRotation *
                Quaternion.Euler(
                    -angle,
                    0f,
                    -angle * 0.5f
                );

            equippedPickaxe.transform.localPosition =
                basePosition +
                new Vector3(
                    0f,
                    0f,
                    Mathf.Sin(swingTimer) * 0.08f
                );
        }

        private void ResetPickaxe()
        {
            if (equippedPickaxe == null)
                return;

            swingTimer = 0f;

            equippedPickaxe.transform.localPosition =
                basePosition;

            equippedPickaxe.transform.localRotation =
                baseRotation;
        }

        public Animator PickaxeAnimator
        {
            get
            {
                if (pickaxeHolder == null)
                    return null;

                return pickaxeHolder
                    .GetComponentInChildren<Animator>();
            }
        }
    }
}

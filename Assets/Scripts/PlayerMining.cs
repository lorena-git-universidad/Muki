using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class PlayerMining : MonoBehaviour
    {
        [Header("References")]
        public Camera playerCamera;
        public Transform pickaxeHolder;

        [Header("Interaction")]
        public float interactionDistance = 3.5f;
        public LayerMask mineableLayer;

        [Header("Pickaxe Position")]
        public Vector3 equippedPosition = new Vector3(0.35f, -0.35f, 0.60f);
        public Vector3 equippedRotation = new Vector3(15f, -80f, 20f);

        [Header("Animation")]
        public float swingSpeed = 12f;
        public float swingAngle = 35f;

        [Header("State")]
        public bool hasPickaxe;

        [HideInInspector]
        public GameObject equippedPickaxe;

        private Quaternion baseRotation;
        private Vector3 basePosition;

        private float swingTimer;
        private bool isMining;

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
                pickaxeHolder = playerCamera.transform.Find("PickaxeHolder");
            }
        }

        private void Update()
        {
            if (!hasPickaxe)
                return;

            CheckMining();
        }

        private void CheckMining()
        {
            Ray ray = new Ray(
                playerCamera.transform.position,
                playerCamera.transform.forward);

            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    interactionDistance,
                    mineableLayer))
            {
                StopMining();
                return;
            }

            MineableRock rock = hit.collider.GetComponentInParent<MineableRock>();

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
            isMining = true;

            AnimatePickaxe();

            bool finished = currentRock.Mine(Time.deltaTime);

            if (finished)
            {
                StopMining();
            }
        }

        private void StopMining()
        {
            if (!isMining)
                return;

            isMining = false;

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
                Debug.LogError("No se encontró PickaxeHolder.");

                return false;
            }

            equippedPickaxe = pickaxeObject;
            hasPickaxe = true;

            Collider[] cols = equippedPickaxe.GetComponentsInChildren<Collider>();

            foreach (Collider c in cols)
            {
                c.enabled = false;
            }

            Rigidbody rb = equippedPickaxe.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            equippedPickaxe.layer = LayerMask.NameToLayer("Ignore Raycast");

            equippedPickaxe.transform.SetParent(pickaxeHolder);

            equippedPickaxe.transform.localPosition = equippedPosition;
            equippedPickaxe.transform.localRotation = Quaternion.Euler(equippedRotation);

            basePosition = equippedPickaxe.transform.localPosition;
            baseRotation = equippedPickaxe.transform.localRotation;

            Debug.Log("Pico equipado.");

            return true;
        }

        private void AnimatePickaxe()
        {
            if (equippedPickaxe == null)
                return;

            swingTimer += Time.deltaTime * swingSpeed;

            float angle = Mathf.Sin(swingTimer) * swingAngle;

            equippedPickaxe.transform.localRotation =
                baseRotation * Quaternion.Euler(-angle, 0f, -angle * 0.5f);

            equippedPickaxe.transform.localPosition =
                basePosition +
                new Vector3(0f, 0f, Mathf.Sin(swingTimer) * 0.08f);
        }

        private void ResetPickaxe()
        {
            if (equippedPickaxe == null)
                return;

            swingTimer = 0f;

            equippedPickaxe.transform.localPosition = basePosition;
            equippedPickaxe.transform.localRotation = baseRotation;
        }
    }
}
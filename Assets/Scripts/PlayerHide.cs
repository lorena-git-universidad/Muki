using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class PlayerHide : MonoBehaviour
    {
        [Header("Referencias")]
        public CharacterController controller;
        public FirstPersonController movement;

        [Tooltip("Objeto que controla la vista del jugador.")]
        public Transform cameraPivot;

        [Tooltip("Objeto raiz del Muki, donde esta MukiEnemy.")]
        public Transform mukiTransform;

        [Header("Posicion y rotacion del escondite")]
        [Tooltip("Si esta activado, la posicion Y final de la capsula sera fija en el mundo.")]
        public bool useFixedWorldY = true;

        [Tooltip("Coordenada Y final de la capsula en el mundo.")]
        public float fixedWorldY = -0.64f;

        [Tooltip("Rotacion X final de la capsula.")]
        public float hiddenRotationX = 180f;

        [Tooltip("Rotacion Z final de la capsula.")]
        public float hiddenRotationZ = 180f;

        [Header("Animacion de entrada")]
        [Tooltip("Duracion total de la animacion de entrada, en segundos.")]
        public float enterAnimationDuration = 1.5f;

        [Tooltip("Altura extra del saltito que hace la capsula al entrar al vagon.")]
        public float jumpHeight = 0.45f;

        [Tooltip("Cuanto sube la camara durante la entrada.")]
        public float cameraRiseHeight = 0.22f;

        [Tooltip("Grados que gira la camara horizontalmente.")]
        public float cameraTurnDegrees = 180f;

        [Tooltip("Desplazamiento local final de la camara dentro del escondite.")]
        public Vector3 hiddenCameraOffset = new Vector3(0f, 0.22f, 0.08f);

        [Header("Proteccion al salir")]
        [Tooltip("Distancia que el Muki debe alejarse antes de empezar a contar la proteccion.")]
        public float mukiMustMoveAwayDistance = 5f;

        [Tooltip("Duracion de la proteccion despues de que el Muki se aleje.")]
        public float protectionDuration = 5f;

        public bool IsHidden { get; private set; }
        public bool IsPostHideProtected { get; private set; }

        private Transform currentHidePoint;

        private Vector3 exitPosition;
        private Quaternion exitRotation;

        private Vector3 originalCameraLocalPosition;
        private Quaternion originalCameraLocalRotation;

        private Coroutine cameraAnimation;
        private Coroutine protectionRoutine;

        private int enterFrame;
        private bool isAnimating;

        private void Awake()
        {
            if (controller == null)
                controller = GetComponent<CharacterController>();

            if (movement == null)
                movement = GetComponent<FirstPersonController>();

            if (mukiTransform == null)
            {
                MukiEnemy muki = FindFirstObjectByType<MukiEnemy>();

                if (muki != null)
                    mukiTransform = muki.transform;
            }
        }

        private void Start()
        {
            if (cameraPivot != null)
            {
                originalCameraLocalPosition = cameraPivot.localPosition;
                originalCameraLocalRotation = cameraPivot.localRotation;
            }
            else
            {
                Debug.LogWarning(
                    "PlayerHide: asigna Camera Pivot en el Inspector."
                );
            }

            if (mukiTransform == null)
            {
                Debug.LogWarning(
                    "PlayerHide: no se encontro el Muki. " +
                    "Asigna Muki Transform en el Inspector."
                );
            }
        }

        private void Update()
        {
            if (!IsHidden || isAnimating)
                return;

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
            else if (hidePoint != null)
            {
                EnterHide(hidePoint);
            }
        }

        private void EnterHide(Transform hidePoint)
        {
            if (isAnimating || hidePoint == null)
                return;

            currentHidePoint = hidePoint;

            // Guardar la posicion y rotacion originales para salir correctamente.
            exitPosition = transform.position;
            exitRotation = transform.rotation;

            if (cameraPivot != null)
            {
                originalCameraLocalPosition = cameraPivot.localPosition;
                originalCameraLocalRotation = cameraPivot.localRotation;
            }

            IsHidden = true;
            enterFrame = Time.frameCount;

            // Desactivar los controles mientras se ejecuta la animacion.
            if (controller != null)
                controller.enabled = false;

            if (movement != null)
                movement.enabled = false;

            // Obtener la posicion final del HidePoint.
            Vector3 targetPosition = hidePoint.position;

            // Opcionalmente fijar la Y en una coordenada del mundo.
            if (useFixedWorldY)
                targetPosition.y = fixedWorldY;

            // Conservar la rotacion Y del HidePoint y fijar X/Z.
            Vector3 hideEuler = hidePoint.rotation.eulerAngles;
            hideEuler.x = hiddenRotationX;
            hideEuler.z = hiddenRotationZ;

            Quaternion targetRotation = Quaternion.Euler(hideEuler);

            if (cameraAnimation != null)
                StopCoroutine(cameraAnimation);

            cameraAnimation = StartCoroutine(
                AnimateEnteringHideout(targetPosition, targetRotation)
            );
        }

        private IEnumerator AnimateEnteringHideout(
            Vector3 targetPosition,
            Quaternion targetRotation)
        {
            isAnimating = true;

            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;

            Vector3 raisedCameraPosition =
                originalCameraLocalPosition +
                Vector3.up * cameraRiseHeight;

            Vector3 finalCameraPosition =
                originalCameraLocalPosition + hiddenCameraOffset;

            Quaternion startCameraRotation = originalCameraLocalRotation;

            Quaternion finalCameraRotation =
                startCameraRotation *
                Quaternion.Euler(0f, cameraTurnDegrees, 0f);

            float duration = Mathf.Max(0.01f, enterAnimationDuration);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                float smoothT = t * t * (3f - 2f * t);

                if (t < 0.5f)
                {
                    // Primera mitad: el cuerpo entra al escondite y la camara sube.
                    float enterT = Mathf.Clamp01(smoothT * 2f);

                    transform.position = EvaluateJumpPosition(
                        startPosition,
                        targetPosition,
                        enterT
                    );

                    transform.rotation = Quaternion.Slerp(
                        startRotation,
                        targetRotation,
                        enterT
                    );

                    if (cameraPivot != null)
                    {
                        cameraPivot.localPosition = Vector3.Lerp(
                            originalCameraLocalPosition,
                            raisedCameraPosition,
                            enterT
                        );

                        cameraPivot.localRotation = startCameraRotation;
                    }
                }
                else
                {
                    // Segunda mitad: el jugador termina de acomodarse y la camara gira.
                    float turnT = Mathf.Clamp01((t - 0.5f) * 2f);
                    turnT = turnT * turnT * (3f - 2f * turnT);

                    transform.position = EvaluateJumpPosition(
                        startPosition,
                        targetPosition,
                        smoothT
                    );

                    transform.rotation = Quaternion.Slerp(
                        startRotation,
                        targetRotation,
                        smoothT
                    );

                    if (cameraPivot != null)
                    {
                        cameraPivot.localPosition = Vector3.Lerp(
                            raisedCameraPosition,
                            finalCameraPosition,
                            turnT
                        );

                        cameraPivot.localRotation = Quaternion.Slerp(
                            startCameraRotation,
                            finalCameraRotation,
                            turnT
                        );
                    }
                }

                yield return null;
            }

            // Asegurar que la posicion y rotacion finales sean exactas.
            transform.SetPositionAndRotation(targetPosition, targetRotation);

            if (cameraPivot != null)
            {
                cameraPivot.localPosition =
                    originalCameraLocalPosition + hiddenCameraOffset;

                cameraPivot.localRotation =
                    originalCameraLocalRotation *
                    Quaternion.Euler(0f, cameraTurnDegrees, 0f);
            }

            isAnimating = false;
            cameraAnimation = null;
        }

        // Trayectoria curva: el jugador sube en un pequeno arco y baja
        // hasta la posicion final, en vez de atravesar el vagon en linea recta.
        private Vector3 EvaluateJumpPosition(
            Vector3 startPosition,
            Vector3 targetPosition,
            float t)
        {
            Vector3 position = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );

            float arc = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            position.y += arc;

            return position;
        }

        private void ExitHide()
        {
            if (!IsHidden || isAnimating)
                return;

            IsHidden = false;

            // Regresar al lugar y orientacion previos a entrar.
            transform.SetPositionAndRotation(exitPosition, exitRotation);

            if (cameraPivot != null)
            {
                cameraPivot.localPosition = originalCameraLocalPosition;
                cameraPivot.localRotation = originalCameraLocalRotation;
            }

            if (controller != null)
                controller.enabled = true;

            if (movement != null)
                movement.enabled = true;

            currentHidePoint = null;

            StartPostHideProtection();
        }

        private void StartPostHideProtection()
        {
            if (protectionRoutine != null)
                StopCoroutine(protectionRoutine);

            protectionRoutine = StartCoroutine(PostHideProtectionRoutine());
        }

        private IEnumerator PostHideProtectionRoutine()
        {
            // La proteccion se activa desde que sales del escondite,
            // y dura hasta que el Muki se aleja y pasan los segundos indicados.
            IsPostHideProtected = true;

            if (mukiTransform != null)
            {
                while (Vector3.Distance(
                    mukiTransform.position,
                    transform.position
                ) < mukiMustMoveAwayDistance)
                {
                    yield return null;
                }
            }

            float timer = 0f;

            while (timer < protectionDuration)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            IsPostHideProtected = false;
            protectionRoutine = null;
        }

        private void OnDisable()
        {
            if (cameraAnimation != null)
                StopCoroutine(cameraAnimation);

            if (protectionRoutine != null)
                StopCoroutine(protectionRoutine);

            isAnimating = false;
            cameraAnimation = null;
            protectionRoutine = null;
            IsPostHideProtected = false;
        }
    }
}

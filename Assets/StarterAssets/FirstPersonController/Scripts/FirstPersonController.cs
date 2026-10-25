using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.0f;
        [Tooltip("Rotation speed of the character")]
        public float RotationSpeed = 1.0f;
        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.1f;
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;
        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.5f;
        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;
        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 90.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -90.0f;

        [Header("Audio - Pasos del jugador")]
        [Tooltip("Nombre EXACTO del sonido de caminar en el AudioManager.")]
        public string walkSoundName = "Caminar";
        [Tooltip("Pitch al caminar.")]
        public float walkPitch = 1f;
        [Tooltip("Pitch al correr (2 = el doble de rapido).")]
        public float sprintPitch = 2f;
        [Tooltip("Pitch al caminar agachado.")]
        public float crouchPitch = 0.75f;
        [Tooltip("Velocidad horizontal minima para que suenen los pasos.")]
        public float minSpeedForFootsteps = 0.5f;

        [Header("Audio - Respiracion agitada")]
        [Tooltip("Nombre del sonido en el AudioManager.")]
        public string breathingSoundName = "respiracion agitada";
        [Tooltip("Segundos corriendo necesarios para que despues suene la respiracion.")]
        public float minSprintTimeForBreathing = 1.5f;
        [Tooltip("Cuanto dura la respiracion despues de dejar de correr.")]
        public float breathingDuration = 6f;
        [Tooltip("Fundido de salida al terminar la respiracion o al salir del escondite.")]
        public float breathingFadeOut = 1f;

        /// <summary>True mientras el jugador corre (sprint + movimiento, sin agacharse).</summary>
        public bool IsSprinting { get; private set; }

        // cinemachine
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // footsteps
        private AudioSource _footstepSource;
        private bool _footstepsInitFailed;

        // breathing
        private AudioSource _breathingSource;
        private bool _hiddenBreathing;
        private float _sprintTime;
        private float _breathingTimer;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            JumpAndGravity();
            GroundedCheck();
            Move();
            UpdateBreathing();
            UpdateFootsteps();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void OnDisable()
        {
            // Si el script se desactiva (por ejemplo al esconderse) ya no se esta corriendo
            // y los pasos se pausan.
            IsSprinting = false;
            _sprintTime = 0f;

            if (_footstepSource != null && _footstepSource.isPlaying)
                _footstepSource.Pause();
        }

        private void OnDestroy()
        {
            if (AudioManager.Instance != null)
            {
                if (_footstepSource != null)
                    AudioManager.Instance.StopLoop(_footstepSource);

                if (_breathingSource != null)
                    AudioManager.Instance.StopLoop(_breathingSource);
            }
        }

        // ==========================================
        // PASOS DEL JUGADOR
        // ==========================================

        private void UpdateFootsteps()
        {
            if (_footstepSource == null)
            {
                if (_footstepsInitFailed || AudioManager.Instance == null)
                    return;

                // Se crea como hijo del jugador y en 2D (spatialBlend 0).
                _footstepSource = AudioManager.Instance.PlayLoop(
                    walkSoundName,
                    transform,
                    0f
                );

                if (_footstepSource == null)
                {
                    _footstepsInitFailed = true; // evita repetir el aviso cada frame
                    return;
                }

                _footstepSource.Pause();
            }

            float horizontalSpeed = new Vector3(
                _controller.velocity.x,
                0f,
                _controller.velocity.z
            ).magnitude;

            bool isMoving =
                Grounded &&
                _input.move != Vector2.zero &&
                horizontalSpeed > minSpeedForFootsteps;

            // Velocidad del audio: normal al caminar, doble al correr
            if (IsSprinting)
                _footstepSource.pitch = sprintPitch;
            else if (_input.crouch)
                _footstepSource.pitch = crouchPitch;
            else
                _footstepSource.pitch = walkPitch;

            if (isMoving)
            {
                if (!_footstepSource.isPlaying)
                    _footstepSource.UnPause();
            }
            else if (_footstepSource.isPlaying)
            {
                _footstepSource.Pause();
            }
        }

        // ==========================================
        // RESPIRACION AGITADA
        // ==========================================

        private void UpdateBreathing()
        {
            bool sprinting =
                _input.sprint &&
                !_input.crouch &&
                _input.move != Vector2.zero;

            IsSprinting = sprinting;

            // Mientras esta escondido la respiracion la controla el escondite.
            if (_hiddenBreathing)
                return;

            if (sprinting)
            {
                _sprintTime += Time.deltaTime;

                // Si vuelve a correr, la respiracion se corta.
                if (_breathingSource != null)
                    StopBreathing(0.3f);
            }
            else
            {
                // Acaba de dejar de correr despues de correr suficiente.
                if (_sprintTime >= minSprintTimeForBreathing)
                    StartBreathing(breathingDuration);

                _sprintTime = 0f;

                if (_breathingSource != null)
                {
                    _breathingTimer -= Time.deltaTime;

                    if (_breathingTimer <= 0f)
                        StopBreathing(breathingFadeOut);
                }
            }
        }

        private void StartBreathing(float duration)
        {
            _breathingTimer = duration;

            if (_breathingSource != null)
                return;

            if (AudioManager.Instance == null)
                return;

            _breathingSource = AudioManager.Instance.PlayLoop(
                breathingSoundName,
                transform,
                0f
            );
        }

        private void StopBreathing(float fade)
        {
            if (_breathingSource == null)
                return;

            if (AudioManager.Instance != null)
                AudioManager.Instance.StopLoop(_breathingSource, fade);

            _breathingSource = null;
        }

        /// <summary>Lo llama PlayerHide al entrar al escondite.</summary>
        public void StartHiddenBreathing()
        {
            _hiddenBreathing = true;
            _sprintTime = 0f;

            if (_breathingSource == null && AudioManager.Instance != null)
            {
                _breathingSource = AudioManager.Instance.PlayLoop(
                    breathingSoundName,
                    transform,
                    0f
                );
            }
        }

        /// <summary>Lo llama PlayerHide al salir del escondite.</summary>
        public void StopHiddenBreathing()
        {
            _hiddenBreathing = false;
            _sprintTime = 0f;

            StopBreathing(breathingFadeOut);
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        private void CameraRotation()
        {
            // if there is an input
            if (_input.look.sqrMagnitude >= _threshold)
            {
                //Don't multiply mouse input by Time.deltaTime
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
                _rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

                // clamp our pitch rotation
                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

                // Update Cinemachine camera target pitch
                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

                // rotate the player left and right
                transform.Rotate(Vector3.up * _rotationVelocity);
            }
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed;

            if (_input.crouch)
            {
                targetSpeed = MoveSpeed * 0.5f;
            }
            else if (_input.sprint)
            {
                targetSpeed = SprintSpeed;
            }
            else
            {
                targetSpeed = MoveSpeed;
            }

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                // move
                inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
            }

            // move the player
            _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }
    }
}
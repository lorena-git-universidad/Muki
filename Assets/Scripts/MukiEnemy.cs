using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace StarterAssets
{
    public class MukiEnemy : MonoBehaviour
    {
        // =========================================================
        // ESTADOS DEL MUKI
        // =========================================================

        public enum EnemyState
        {
            Patrolling,
            Chasing,
            Searching,
            Calmed,
            InvestigatingNoise
        }

        [Header("Estado actual")]
        public EnemyState currentState = EnemyState.Patrolling;

        // =========================================================
        // REFERENCIAS
        // =========================================================

        [Header("Referencias")]
        public Transform player;
        public NavMeshAgent agent;
        public PlayerHide playerHide;
        public Animator animator;

        // =========================================================
        // PATRULLA
        // =========================================================

        [Header("Patrulla")]
        public Transform[] patrolPoints;
        public float patrolSpeed = 2.5f;
        public float patrolWaitTime = 2f;

        private int currentPatrolIndex = 0;
        private float patrolTimer = 0f;

        // =========================================================
        // DETECCIÓN DEL JUGADOR
        // =========================================================

        [Header("Detección")]
        public float detectionRange = 12f;
        public float losePlayerRange = 18f;

        // =========================================================
        // PERSECUCIÓN
        // =========================================================

        [Header("Persecución")]
        public float chaseSpeed = 5f;

        // =========================================================
        // ATAQUE
        // =========================================================

        [Header("Ataque")]
        public float attackDistance = 1.5f;
        public float attackCooldown = 2f;

        private float attackTimer = 0f;

        [Header("Videos de screamer")]
        public MukiScreamerVideos screamerVideos;

        [Tooltip("Tiempo que dura la animación de ataque antes de reaparecer al jugador.")]
        public float attackAnimationDuration = 1.2f;

        private bool isPerformingAttack = false;

        // =========================================================
        // PROTECCIÓN DESPUÉS DE MATAR
        // =========================================================

        [Header("Después de matar al jugador")]
        [Tooltip("Tiempo durante el cual Muki no vuelve a detectar ni atacar al jugador.")]
        public float postKillProtectionTime = 10f;

        [Tooltip("Distancia mínima deseada entre Muki y el jugador después de matarlo.")]
        public float postKillMinDistance = 20f;

        private float postKillProtectionTimer = 0f;

        // =========================================================
        // INVESTIGACIÓN
        // =========================================================

        [Header("Investigación de ruido")]
        public float searchTime = 3f;

        [Tooltip("Tiempo que Muki permanece quieto investigando el punto del ruido.")]
        public float investigationWaitTime = 3f;

        private float searchTimer = 0f;
        private bool investigatingNoise = false;
        private Transform noisePatrolPoint;
        private bool waitingAtNoisePoint = false;
        private float noiseWaitTimer = 0f;

        // =========================================================
        // ALTARES
        // =========================================================

        [Header("Altares")]
        public OfferingAltar[] altars;

        private Transform calmPatrolTarget;

        // =========================================================
        // ZONAS SEGURAS PERMANENTES
        // =========================================================

        [Header("Permanent Safe Zones")]
        public MukiSafeZone[] safeZones;

        // =========================================================
        // AUDIO
        // =========================================================

        [Header("AI Audio")]
        public AudioClip investigationStartSound;
        public AudioClip investigationLoopSound;
        public AudioClip chaseLaughSound;

        private AudioSource oneShotAudio;
        private AudioSource stateLoopAudio;
        private EnemyState lastAudioState;
        private bool audioStateInitialized = false;

        // ---------------------------------------------------------
        // AUDIO NUEVO (se reproducen con el AudioManager por nombre)
        // ---------------------------------------------------------

        [Header("Audio nuevo - Inicio de persecucion")]
        public string illFoundYouSoundName = "illfoundyou";

        [Tooltip("Indica si illfoundyou ya se reprodujo durante esta instancia de Muki.")]
        private bool illFoundYouPlayed = false;

        [Header("Audio nuevo - Pasos del Muki")]
        public string footstepsSoundName = "Pasos_Muki";

        [Tooltip("Pitch de los pasos al patrullar / investigar.")]
        public float walkFootstepPitch = 1f;

        [Tooltip("Pitch de los pasos al perseguir (1.5 a 2).")]
        public float chaseFootstepPitch = 1.5f;

        public float footstepMinDistance = 2f;
        public float footstepMaxDistance = 25f;

        [Header("Audio nuevo - Sonido ambiente del Muki")]
        public string ambientSoundName = "Sonidos muki";

        [Tooltip("Tiempo minimo (s) entre sonidos ambiente.")]
        public float ambientMinInterval = 90f;

        [Tooltip("Tiempo maximo (s) entre sonidos ambiente.")]
        public float ambientMaxInterval = 150f;

        private AudioSource footstepSource;
        private bool footstepsInitFailed = false;
        private float ambientTimer;

        // =========================================================
        // AWAKE
        // =========================================================

        private void Awake()
        {

            if (screamerVideos == null)
                screamerVideos = FindFirstObjectByType<MukiScreamerVideos>();

            if (agent == null)
                agent = GetComponent<NavMeshAgent>();

            if (playerHide == null)
                playerHide = FindFirstObjectByType<PlayerHide>();

            if (player == null)
            {
                GameObject playerObject =
                    GameObject.FindGameObjectWithTag("Player");

                if (playerObject != null)
                    player = playerObject.transform;
            }

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (altars == null || altars.Length == 0)
            {
                altars =
                    FindObjectsByType<OfferingAltar>(
                        FindObjectsSortMode.None
                    );
            }

            if (safeZones == null || safeZones.Length == 0)
            {
                safeZones =
                    FindObjectsByType<MukiSafeZone>(
                        FindObjectsSortMode.None
                    );
            }

            // AUDIO ONE SHOT
            oneShotAudio = gameObject.AddComponent<AudioSource>();
            oneShotAudio.playOnAwake = false;
            oneShotAudio.loop = false;
            oneShotAudio.spatialBlend = 0f;

            // AUDIO LOOP
            stateLoopAudio = gameObject.AddComponent<AudioSource>();
            stateLoopAudio.playOnAwake = false;
            stateLoopAudio.loop = true;
            stateLoopAudio.spatialBlend = 0f;
        }

        // =========================================================
        // START
        // =========================================================

        private void Start()
        {
            currentState = EnemyState.Patrolling;
            agent.speed = patrolSpeed;
            agent.isStopped = false;

            // AUDIO NUEVO: primer sonido ambiente aleatorio
            ResetAmbientTimer();

            GoToNextPatrolPoint();
            UpdateAnimations();
        }

        // =========================================================
        // UPDATE
        // =========================================================

        private void Update()
        {
            if (player == null)
                return;

            // AUDIO
            UpdateStateAudio();
            UpdateAmbientSound(); // AUDIO NUEVO

            // ATTACK COOLDOWN
            if (attackTimer > 0f)
                attackTimer -= Time.deltaTime;

            // PROTECCIÓN DESPUÉS DE MATAR
            if (postKillProtectionTimer > 0f)
                postKillProtectionTimer -= Time.deltaTime;

            // ALTAR ACTIVO
            if (IsAnyAltarActive())
            {
                if (currentState != EnemyState.Calmed)
                    StartCalmed();

                Calm();
                UpdateAnimations();

                return;
            }

            // ESTADOS
            switch (currentState)
            {
                case EnemyState.Patrolling:
                    Patrol();
                    break;

                case EnemyState.Chasing:
                    ChasePlayer();
                    break;

                case EnemyState.Searching:
                    Search();
                    break;

                case EnemyState.Calmed:
                    EndCalmed();
                    break;

                case EnemyState.InvestigatingNoise:
                    InvestigateNoise();
                    break;
            }

            // ANIMACIONES (y pasos)
            UpdateAnimations();
        }

        private void OnDisable()
        {
            // AUDIO NUEVO: si la IA se apaga (game over) los pasos se pausan
            if (footstepSource != null && footstepSource.isPlaying)
                footstepSource.Pause();
        }

        private void OnDestroy()
        {
            if (footstepSource != null && AudioManager.Instance != null)
                AudioManager.Instance.StopLoop(footstepSource);
        }

        // =========================================================
        // ANIMACIONES
        // =========================================================

        private void UpdateAnimations()
        {
            // ¿MUKI SE ESTÁ MOVIENDO?
            bool isWalking =
                agent != null &&
                !agent.isStopped &&
                agent.velocity.magnitude > 0.1f;

            // ¿ESTÁ PERSIGUIENDO?
            bool isChasing =
                currentState == EnemyState.Chasing;

            // AUDIO NUEVO: pasos
            UpdateFootsteps(isWalking, isChasing);

            if (animator == null)
                return;

            animator.SetBool("IsWalking", isWalking);
            animator.SetBool("IsChasing", isChasing);
        }

        // =========================================================
        // AUDIO NUEVO: PASOS
        // =========================================================

        private void UpdateFootsteps(bool isWalking, bool isChasing)
        {
            if (footstepSource == null)
            {
                if (footstepsInitFailed || AudioManager.Instance == null)
                    return;

                footstepSource = AudioManager.Instance.PlayLoop(
                    footstepsSoundName,
                    transform,
                    1f, // 3D: se oye desde donde esta el Muki
                    footstepMinDistance,
                    footstepMaxDistance
                );

                if (footstepSource == null)
                {
                    footstepsInitFailed = true; // evita repetir el aviso cada frame
                    return;
                }
            }

            // Velocidad normal al caminar, mas rapida al perseguir
            footstepSource.pitch =
                isChasing ? chaseFootstepPitch : walkFootstepPitch;

            if (isWalking)
            {
                if (!footstepSource.isPlaying)
                    footstepSource.UnPause();
            }
            else if (footstepSource.isPlaying)
            {
                footstepSource.Pause();
            }
        }

        // =========================================================
        // AUDIO NUEVO: SONIDO AMBIENTE ALEATORIO
        // =========================================================

        private void ResetAmbientTimer()
        {
            float min = Mathf.Max(1f, ambientMinInterval);
            float max = Mathf.Max(min, ambientMaxInterval);

            ambientTimer = Random.Range(min, max);
        }

        private void UpdateAmbientSound()
        {
            // No se cuenta el tiempo durante persecucion o ataque
            if (currentState == EnemyState.Chasing || isPerformingAttack)
                return;

            ambientTimer -= Time.deltaTime;

            if (ambientTimer > 0f)
                return;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySound(ambientSoundName);

            ResetAmbientTimer();
        }

        // =========================================================
        // PATRULLA
        // =========================================================

        private void Patrol()
        {
            agent.speed = patrolSpeed;
            agent.isStopped = false;

            // Durante la protección Muki NO detecta ni persigue al jugador.
            if (postKillProtectionTimer <= 0f)
            {
                if (IsPlayerDetectable())
                {
                    float distanceToPlayer =
                        Vector3.Distance(transform.position, player.position);

                    if (distanceToPlayer <= detectionRange)
                    {
                        StartChasing();
                        return;
                    }
                }
            }

            // SIGUIENTE PUNTO
            if (!agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance)
            {
                patrolTimer += Time.deltaTime;

                if (patrolTimer >= patrolWaitTime)
                {
                    patrolTimer = 0f;
                    GoToNextPatrolPoint();
                }
            }
        }

        // =========================================================
        // SIGUIENTE PUNTO DE PATRULLA
        // =========================================================

        private void GoToNextPatrolPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
                return;

            if (currentPatrolIndex >= patrolPoints.Length)
                currentPatrolIndex = 0;

            Transform target = patrolPoints[currentPatrolIndex];

            currentPatrolIndex++;

            if (target != null)
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
        }

        // =========================================================
        // COMENZAR PERSECUCIÓN
        // =========================================================

        private void StartChasing()
        {
            // Durante la protección no puede perseguir.
            if (postKillProtectionTimer > 0f)
                return;

            if (isPerformingAttack)
                return;

            currentState = EnemyState.Chasing;
            agent.speed = chaseSpeed;
            agent.isStopped = false;

            Debug.Log("Muki comenzó a perseguir al jugador.");
        }

        // =========================================================
        // PERSEGUIR
        // =========================================================

        private void ChasePlayer()
        {

            // No mover ni actualizar la persecución durante el ataque.
            if (isPerformingAttack)
                return;

            // PROTECCIÓN
            if (postKillProtectionTimer > 0f)
            {
                currentState = EnemyState.Patrolling;
                agent.speed = patrolSpeed;
                agent.isStopped = false;
                GoToNextPatrolPoint();
                return;
            }

            // ZONA SEGURA
            if (IsPlayerInsideSafeZone())
            {
                currentState = EnemyState.Patrolling;
                agent.speed = patrolSpeed;
                agent.isStopped = false;
                GoToNextPatrolPoint();

                Debug.Log(
                    "Muki abandonó la persecución: " +
                    "jugador en zona segura."
                );

                return;
            }

            // ESCONDIDO
            if (playerHide != null && playerHide.IsHidden)
            {
                StartSearching();
                return;
            }

            // DISTANCIA
            float distanceToPlayer =
                Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer > losePlayerRange)
            {
                StartSearching();
                return;
            }

            // SEGUIR AL JUGADOR
            agent.speed = chaseSpeed;
            agent.isStopped = false;
            agent.SetDestination(player.position);

            // ATAQUE
            if (distanceToPlayer <= attackDistance)
                TryAttack();
        }

        // =========================================================
        // ATAQUE / MUERTE DEL JUGADOR
        // =========================================================

        private void TryAttack()
        {
            if (attackTimer > 0f)
                return;

            if (isPerformingAttack)
                return;

            if (postKillProtectionTimer > 0f)
                return;

            StartCoroutine(PerformAttack());
        }

        // =========================================================
        // CORRUTINA DEL ATAQUE
        // =========================================================



        private IEnumerator PerformAttack()
        {
            isPerformingAttack = true;

            // 1. Iniciar el cooldown del ataque.
            attackTimer = attackCooldown;

            // 2. Detener a Muki mientras ocurre el ataque.
            if (agent != null)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            currentState = EnemyState.Chasing;

            // =====================================================
            // 3. EJECUTAR PRIMERO LA ANIMACIÓN DE ATAQUE
            // =====================================================

            if (animator != null)
            {
                animator.ResetTrigger("Attack");
                animator.SetTrigger("Attack");
            }

            Debug.Log("Muki comienza su animación de ataque.");

            // Esperar a que termine la animación.
            yield return new WaitForSeconds(attackAnimationDuration);

            Debug.Log("Animación terminada. Ahora comienza el screamer.");

            // =====================================================
            // 4. REPRODUCIR EL SCREAMER DESPUÉS DE LA ANIMACIÓN
            // =====================================================

            if (screamerVideos != null &&
                screamerVideos.HasAttackScreamer)
            {
                yield return StartCoroutine(
                    screamerVideos.PlayAttackScreamer()
                );
            }
            else
            {
                Debug.LogWarning(
                    "No se encontró el screamer de ataque o no está asignado."
                );
            }

            // =====================================================
            // 5. REGISTRAR LA CAPTURA Y DESCONTAR UNA VIDA
            // =====================================================

            if (PlayerLives.Instance == null)
            {
                Debug.LogError(
                    "No se encontró PlayerLives. " +
                    "Añade el componente a un objeto de la escena."
                );

                isPerformingAttack = false;
                yield break;
            }

            bool gameOver = PlayerLives.Instance.RegisterCapture();

            // =====================================================
            // 6. SI PERDIÓ LA ÚLTIMA VIDA, MOSTRAR EL VIDEO FINAL
            // =====================================================

            if (gameOver)
            {
                if (agent != null)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                }

                Debug.Log(
                    "Última vida perdida. Reproduciendo el screamer final."
                );

                // Evitar que Muki siga ejecutando su IA.
                enabled = false;

                if (screamerVideos != null &&
                    screamerVideos.HasFinalDeathScreamer)
                {
                    yield return StartCoroutine(
                        screamerVideos.PlayFinalDeathScreamer()
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "No hay screamer final asignado. " +
                        "Se utilizará el tiempo de espera habitual."
                    );

                    yield return new WaitForSecondsRealtime(
                        PlayerLives.Instance.defeatDelay
                    );
                }

                // Cargar Derrota después de terminar el video final.
                PlayerLives.Instance.LoadDefeatSceneNow();
                yield break;
            }

            // =====================================================
            // 7. SI QUEDAN VIDAS, REAPARECER EN EL CHECKPOINT
            // =====================================================

            if (PlayerRespawn.Instance != null)
            {
                PlayerRespawn.Instance.Respawn();
            }
            else
            {
                Debug.LogWarning(
                    "No existe PlayerRespawn.Instance."
                );
            }

            // =====================================================
            // 8. ACTIVAR PROTECCIÓN Y ALEJAR A MUKI
            // =====================================================

            postKillProtectionTimer = postKillProtectionTime;

            investigatingNoise = false;
            noisePatrolPoint = null;
            waitingAtNoisePoint = false;
            noiseWaitTimer = 0f;
            calmPatrolTarget = null;
            patrolTimer = 0f;

            Transform safePatrolPoint = GetSafePostKillPatrolPoint();

            if (safePatrolPoint != null)
            {
                MoveMukiToPatrolPoint(safePatrolPoint);
            }
            else
            {
                currentState = EnemyState.Patrolling;

                if (agent != null)
                {
                    agent.speed = patrolSpeed;
                    agent.isStopped = false;
                }

                GoToNextPatrolPoint();
            }

            isPerformingAttack = false;

            Debug.Log(
                "Muki se alejó después de la captura. " +
                "Protección activa durante " +
                postKillProtectionTime + " segundos."
            );
        }



        // =========================================================
        // MOVER MUKI A UN PUNTO LEJANO
        // =========================================================

        private void MoveMukiToPatrolPoint(Transform target)
        {
            if (target == null)
                return;

            NavMeshHit navHit;

            // BUSCAR POSICIÓN VÁLIDA EN NAVMESH
            if (NavMesh.SamplePosition(
                target.position,
                out navHit,
                3f,
                NavMesh.AllAreas))
            {
                agent.Warp(navHit.position);
            }

            // ACTUALIZAR ÍNDICE
            currentPatrolIndex = GetPatrolPointIndex(target);

            // VOLVER A PATRULLAR
            currentState = EnemyState.Patrolling;
            agent.speed = patrolSpeed;
            agent.isStopped = false;
            agent.ResetPath();

            // Continuar hacia el siguiente punto.
            GoToNextPatrolPoint();
        }

        // =========================================================
        // OBTENER ÍNDICE DEL PUNTO
        // =========================================================

        private int GetPatrolPointIndex(Transform target)
        {
            if (patrolPoints == null)
                return 0;

            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == target)
                    return i;
            }

            return 0;
        }

        // =========================================================
        // OBTENER PUNTO SEGURO DESPUÉS DE MATAR
        // =========================================================

        private Transform GetSafePostKillPatrolPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
                return null;

            Transform safePoint = null;
            float bestDistance = -1f;

            // PRIMERO: BUSCAR UNO QUE ESTÉ REALMENTE LEJOS
            foreach (Transform point in patrolPoints)
            {
                if (point == null)
                    continue;

                float distance =
                    Vector3.Distance(point.position, player.position);

                if (distance >= postKillMinDistance &&
                    distance > bestDistance)
                {
                    bestDistance = distance;
                    safePoint = point;
                }
            }

            if (safePoint != null)
                return safePoint;

            // SI NINGUNO CUMPLE, USAR EL MÁS LEJANO
            return GetFarthestPatrolPoint();
        }

        // =========================================================
        // COMENZAR BÚSQUEDA
        // =========================================================

        private void StartSearching()
        {
            currentState = EnemyState.Searching;
            searchTimer = searchTime;
            agent.speed = patrolSpeed;
            agent.isStopped = false;
            agent.ResetPath();

            Debug.Log("Muki está buscando al jugador.");
        }

        // =========================================================
        // BUSCAR
        // =========================================================

        private void Search()
        {
            // DETECTAR NUEVAMENTE
            if (postKillProtectionTimer <= 0f)
            {
                if (IsPlayerDetectable())
                {
                    float distanceToPlayer =
                        Vector3.Distance(transform.position, player.position);

                    if (distanceToPlayer <= detectionRange)
                    {
                        StartChasing();
                        return;
                    }
                }
            }

            // TEMPORIZADOR
            searchTimer -= Time.deltaTime;

            if (searchTimer <= 0f)
            {
                currentState = EnemyState.Patrolling;
                agent.speed = patrolSpeed;
                patrolTimer = 0f;
                GoToNextPatrolPoint();
            }
        }

        // =========================================================
        // RECIBIR RUIDO
        // =========================================================

        public void HearNoise(
            Vector3 noisePosition,
            float noiseRadius,
            string noiseType)
        {
            // PROTECCIÓN
            if (postKillProtectionTimer > 0f)
                return;

            // ATAQUE
            if (isPerformingAttack)
                return;

            // ALTAR
            if (IsAnyAltarActive())
                return;

            // ZONA SEGURA
            if (IsPlayerInsideSafeZone())
                return;

            // YA PERSIGUE
            if (currentState == EnemyState.Chasing)
                return;

            // YA INVESTIGA
            if (currentState == EnemyState.InvestigatingNoise)
                return;

            // DISTANCIA AL RUIDO
            float distanceToNoise =
                Vector3.Distance(transform.position, noisePosition);

            if (distanceToNoise > noiseRadius)
                return;

            // PUNTO MÁS CERCANO
            Transform closestPoint = null;
            float closestDistance = Mathf.Infinity;

            if (patrolPoints != null)
            {
                foreach (Transform point in patrolPoints)
                {
                    if (point == null)
                        continue;

                    float distance =
                        Vector3.Distance(point.position, noisePosition);

                    if (distance <= noiseRadius &&
                        distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPoint = point;
                    }
                }
            }

            if (closestPoint == null)
                return;

            // COMENZAR INVESTIGACIÓN
            investigatingNoise = true;
            waitingAtNoisePoint = false;
            noiseWaitTimer = 0f;
            noisePatrolPoint = closestPoint;

            currentState = EnemyState.InvestigatingNoise;
            agent.speed = patrolSpeed;
            agent.isStopped = false;
            agent.SetDestination(closestPoint.position);

            Debug.Log("Muki escuchó un ruido: " + noiseType);
        }

        // =========================================================
        // INVESTIGAR RUIDO
        // =========================================================

        private void InvestigateNoise()
        {
            // ESCONDIDO
            if (playerHide != null && playerHide.IsHidden)
                return;

            // ZONA SEGURA
            if (IsPlayerInsideSafeZone())
            {
                investigatingNoise = false;
                waitingAtNoisePoint = false;
                noisePatrolPoint = null;

                currentState = EnemyState.Patrolling;
                agent.speed = patrolSpeed;
                agent.isStopped = false;
                GoToNextPatrolPoint();

                return;
            }

            // DETECTAR JUGADOR
            if (!waitingAtNoisePoint && IsPlayerDetectable())
            {
                float distanceToPlayer =
                    Vector3.Distance(transform.position, player.position);

                if (distanceToPlayer <= detectionRange)
                {
                    investigatingNoise = false;
                    noisePatrolPoint = null;

                    StartChasing();

                    return;
                }
            }

            // SIN DESTINO
            if (noisePatrolPoint == null)
            {
                investigatingNoise = false;
                waitingAtNoisePoint = false;

                currentState = EnemyState.Patrolling;
                agent.isStopped = false;
                GoToNextPatrolPoint();

                return;
            }

            // YA ESTÁ ESPERANDO EN EL PUNTO
            if (waitingAtNoisePoint)
            {
                agent.isStopped = true;
                noiseWaitTimer -= Time.deltaTime;

                if (noiseWaitTimer <= 0f)
                {
                    waitingAtNoisePoint = false;
                    investigatingNoise = false;
                    noisePatrolPoint = null;

                    currentState = EnemyState.Patrolling;
                    patrolTimer = 0f;
                    agent.isStopped = false;
                    GoToNextPatrolPoint();

                    Debug.Log(
                        "Muki terminó de investigar " +
                        "y continúa patrullando."
                    );
                }

                return;
            }

            // LLEGÓ AL PUNTO DEL RUIDO
            if (!agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance)
            {
                Debug.Log(
                    "Muki llegó al lugar del ruido. " +
                    "Ahora esperará."
                );

                agent.isStopped = true;
                agent.ResetPath();

                waitingAtNoisePoint = true;
                noiseWaitTimer = investigationWaitTime;

                // IsWalking pasa a FALSE y el Animator reproduce IDLE.
                return;
            }
        }

        // =========================================================
        // JUGADOR DETECTABLE
        // =========================================================

        private bool IsPlayerDetectable()
        {
            if (postKillProtectionTimer > 0f)
                return false;

            if (isPerformingAttack)
                return false;

            if (IsAnyAltarActive())
                return false;

            if (playerHide != null && playerHide.IsHidden)
                return false;

            if (IsPlayerInsideSafeZone())
                return false;

            return true;
        }

        // =========================================================
        // JUGADOR EN ZONA SEGURA
        // =========================================================

        private bool IsPlayerInsideSafeZone()
        {
            if (player == null)
                return false;

            if (safeZones == null)
                return false;

            foreach (MukiSafeZone zone in safeZones)
            {
                if (zone == null)
                    continue;

                if (zone.Contains(player.position))
                    return true;
            }

            return false;
        }

        // =========================================================
        // ALTAR ACTIVO
        // =========================================================

        private bool IsAnyAltarActive()
        {
            if (altars == null)
                return false;

            foreach (OfferingAltar altar in altars)
            {
                if (altar == null)
                    continue;

                if (altar.isActive)
                    return true;
            }

            return false;
        }

        // =========================================================
        // COMENZAR CALMADO
        // =========================================================

        private void StartCalmed()
        {
            currentState = EnemyState.Calmed;
            agent.speed = patrolSpeed;
            agent.isStopped = false;
            agent.ResetPath();

            calmPatrolTarget = GetFarthestPatrolPoint();

            if (calmPatrolTarget != null)
                agent.SetDestination(calmPatrolTarget.position);

            Debug.Log(
                "Muki está calmado: " +
                "patrulla lejos del jugador " +
                "y no ataca."
            );
        }

        // =========================================================
        // CALMADO
        // =========================================================

        private void Calm()
        {
            agent.speed = patrolSpeed;
            agent.isStopped = false;

            if (calmPatrolTarget == null)
            {
                calmPatrolTarget = GetFarthestPatrolPoint();

                if (calmPatrolTarget != null)
                    agent.SetDestination(calmPatrolTarget.position);

                return;
            }

            if (!agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance)
            {
                calmPatrolTarget = GetFarthestPatrolPoint();

                if (calmPatrolTarget != null)
                    agent.SetDestination(calmPatrolTarget.position);
            }
        }

        // =========================================================
        // PUNTO MÁS LEJANO DEL JUGADOR
        // =========================================================

        private Transform GetFarthestPatrolPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
                return null;

            Transform farthestPoint = null;
            float greatestDistance = -1f;

            foreach (Transform point in patrolPoints)
            {
                if (point == null)
                    continue;

                if (point == calmPatrolTarget && patrolPoints.Length > 1)
                    continue;

                float distance = 0f;

                if (player != null)
                {
                    distance =
                        Vector3.Distance(point.position, player.position);
                }

                if (distance > greatestDistance)
                {
                    greatestDistance = distance;
                    farthestPoint = point;
                }
            }

            if (farthestPoint == null)
                farthestPoint = calmPatrolTarget;

            return farthestPoint;
        }

        // =========================================================
        // TERMINAR CALMADO
        // =========================================================

        private void EndCalmed()
        {
            currentState = EnemyState.Patrolling;
            agent.speed = patrolSpeed;
            agent.isStopped = false;

            calmPatrolTarget = null;
            patrolTimer = 0f;

            GoToNextPatrolPoint();

            Debug.Log(
                "El efecto del altar terminó. " +
                "Muki vuelve a patrullar normalmente."
            );
        }

        // =========================================================
        // AUDIO POR ESTADO
        // =========================================================

        private void UpdateStateAudio()
        {
            // No hacer nada si el estado no cambió
            if (audioStateInitialized && lastAudioState == currentState)
                return;

            lastAudioState = currentState;
            audioStateInitialized = true;

            // DETENER LOOP ANTERIOR
            if (stateLoopAudio != null)
                stateLoopAudio.Stop();

            // INVESTIGACIÓN
            if (currentState == EnemyState.InvestigatingNoise)
            {
                if (investigationStartSound != null)
                    oneShotAudio.PlayOneShot(investigationStartSound);

                if (investigationLoopSound != null)
                {
                    stateLoopAudio.clip = investigationLoopSound;
                    stateLoopAudio.loop = true;
                    stateLoopAudio.Play();
                }
            }

            // PERSECUCIÓN
            else if (currentState == EnemyState.Chasing)
            {
                // AUDIO NUEVO: reproducir "illfoundyou" una sola vez
                // durante esta instancia de Muki, aunque vuelva a perseguir.
                if (!illFoundYouPlayed && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(illFoundYouSoundName);
                    illFoundYouPlayed = true;
                    Debug.Log("Muki: el sonido illfoundyou ya se reprodujo y no volverá a sonar.");
                }

                if (chaseLaughSound != null)
                {
                    stateLoopAudio.clip = chaseLaughSound;
                    stateLoopAudio.loop = true;
                    stateLoopAudio.Play();
                }
            }

            // OTROS ESTADOS: el loop ya fue detenido arriba.
        }
    }
}
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


        // =========================================================
        // AWAKE
        // =========================================================

        private void Awake()
        {
            // -----------------------------------------------------
            // NAVMESH AGENT
            // -----------------------------------------------------

            if (agent == null)
                agent = GetComponent<NavMeshAgent>();


            // -----------------------------------------------------
            // PLAYER HIDE
            // -----------------------------------------------------

            if (playerHide == null)
                playerHide = FindFirstObjectByType<PlayerHide>();


            // -----------------------------------------------------
            // PLAYER
            // -----------------------------------------------------

            if (player == null)
            {
                GameObject playerObject =
                    GameObject.FindGameObjectWithTag("Player");

                if (playerObject != null)
                    player = playerObject.transform;
            }


            // -----------------------------------------------------
            // ANIMATOR
            // -----------------------------------------------------

            if (animator == null)
                animator = GetComponentInChildren<Animator>();


            // -----------------------------------------------------
            // ALTARES
            // -----------------------------------------------------

            if (altars == null || altars.Length == 0)
            {
                altars =
                    FindObjectsByType<OfferingAltar>(
                        FindObjectsSortMode.None
                    );
            }


            // -----------------------------------------------------
            // ZONAS SEGURAS
            // -----------------------------------------------------

            if (safeZones == null || safeZones.Length == 0)
            {
                safeZones =
                    FindObjectsByType<MukiSafeZone>(
                        FindObjectsSortMode.None
                    );
            }


            // =====================================================
            // AUDIO ONE SHOT
            // =====================================================

            oneShotAudio =
                gameObject.AddComponent<AudioSource>();

            oneShotAudio.playOnAwake = false;

            oneShotAudio.loop = false;

            oneShotAudio.spatialBlend = 0f;


            // =====================================================
            // AUDIO LOOP
            // =====================================================

            stateLoopAudio =
                gameObject.AddComponent<AudioSource>();

            stateLoopAudio.playOnAwake = false;

            stateLoopAudio.loop = true;

            stateLoopAudio.spatialBlend = 0f;
        }


        // =========================================================
        // START
        // =========================================================

        private void Start()
        {
            currentState =
                EnemyState.Patrolling;

            agent.speed =
                patrolSpeed;

            agent.isStopped =
                false;

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


            // -----------------------------------------------------
            // AUDIO
            // -----------------------------------------------------

            UpdateStateAudio();


            // -----------------------------------------------------
            // ATTACK COOLDOWN
            // -----------------------------------------------------

            if (attackTimer > 0f)
                attackTimer -= Time.deltaTime;


            // -----------------------------------------------------
            // PROTECCIÓN DESPUÉS DE MATAR
            // -----------------------------------------------------

            if (postKillProtectionTimer > 0f)
            {
                postKillProtectionTimer -=
                    Time.deltaTime;
            }


            // =====================================================
            // ALTAR ACTIVO
            // =====================================================

            if (IsAnyAltarActive())
            {
                if (currentState != EnemyState.Calmed)
                {
                    StartCalmed();
                }

                Calm();

                UpdateAnimations();

                return;
            }


            // =====================================================
            // ESTADOS
            // =====================================================

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


            // =====================================================
            // ANIMACIONES
            // =====================================================

            UpdateAnimations();
        }


        // =========================================================
        // ANIMACIONES
        // =========================================================

        private void UpdateAnimations()
        {
            if (animator == null)
                return;


            // -----------------------------------------------------
            // ¿MUKI SE ESTÁ MOVIENDO?
            // -----------------------------------------------------

            bool isWalking =
                agent != null &&
                !agent.isStopped &&
                agent.velocity.magnitude > 0.1f;


            // -----------------------------------------------------
            // ¿ESTÁ PERSIGUIENDO?
            // -----------------------------------------------------

            bool isChasing =
                currentState ==
                EnemyState.Chasing;


            // -----------------------------------------------------
            // ENVIAR DATOS AL ANIMATOR
            // -----------------------------------------------------

            animator.SetBool(
                "IsWalking",
                isWalking
            );

            animator.SetBool(
                "IsChasing",
                isChasing
            );
        }


        // =========================================================
        // PATRULLA
        // =========================================================

        private void Patrol()
        {
            agent.speed =
                patrolSpeed;

            agent.isStopped =
                false;


            // -----------------------------------------------------
            // PROTECCIÓN DESPUÉS DE MATAR
            // -----------------------------------------------------

            // Durante estos segundos Muki puede seguir caminando,
            // pero NO detectará ni perseguirá al jugador.

            if (postKillProtectionTimer <= 0f)
            {
                if (IsPlayerDetectable())
                {
                    float distanceToPlayer =
                        Vector3.Distance(
                            transform.position,
                            player.position
                        );


                    if (distanceToPlayer <=
                        detectionRange)
                    {
                        StartChasing();

                        return;
                    }
                }
            }


            // -----------------------------------------------------
            // SIGUIENTE PUNTO
            // -----------------------------------------------------

            if (!agent.pathPending &&
                agent.remainingDistance <=
                agent.stoppingDistance)
            {
                patrolTimer +=
                    Time.deltaTime;


                if (patrolTimer >=
                    patrolWaitTime)
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
            if (patrolPoints == null ||
                patrolPoints.Length == 0)
            {
                return;
            }


            if (currentPatrolIndex >=
                patrolPoints.Length)
            {
                currentPatrolIndex = 0;
            }


            Transform target =
                patrolPoints[currentPatrolIndex];


            currentPatrolIndex++;


            if (target != null)
            {
                agent.isStopped = false;

                agent.SetDestination(
                    target.position
                );
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


            currentState =
                EnemyState.Chasing;

            agent.speed =
                chaseSpeed;

            agent.isStopped =
                false;


            Debug.Log(
                "Muki comenzó a perseguir al jugador."
            );
        }


        // =========================================================
        // PERSEGUIR
        // =========================================================

        private void ChasePlayer()
        {
            // -----------------------------------------------------
            // PROTECCIÓN
            // -----------------------------------------------------

            if (postKillProtectionTimer > 0f)
            {
                currentState =
                    EnemyState.Patrolling;

                agent.speed =
                    patrolSpeed;

                agent.isStopped =
                    false;

                GoToNextPatrolPoint();

                return;
            }


            // -----------------------------------------------------
            // ZONA SEGURA
            // -----------------------------------------------------

            if (IsPlayerInsideSafeZone())
            {
                currentState =
                    EnemyState.Patrolling;

                agent.speed =
                    patrolSpeed;

                agent.isStopped =
                    false;

                GoToNextPatrolPoint();


                Debug.Log(
                    "Muki abandonó la persecución: " +
                    "jugador en zona segura."
                );

                return;
            }


            // -----------------------------------------------------
            // ESCONDIDO
            // -----------------------------------------------------

            if (playerHide != null &&
                playerHide.IsHidden)
            {
                StartSearching();

                return;
            }


            // -----------------------------------------------------
            // DISTANCIA
            // -----------------------------------------------------

            float distanceToPlayer =
                Vector3.Distance(
                    transform.position,
                    player.position
                );


            if (distanceToPlayer >
                losePlayerRange)
            {
                StartSearching();

                return;
            }


            // -----------------------------------------------------
            // SEGUIR AL JUGADOR
            // -----------------------------------------------------

            agent.speed =
                chaseSpeed;

            agent.isStopped =
                false;

            agent.SetDestination(
                player.position
            );


            // -----------------------------------------------------
            // ATAQUE
            // -----------------------------------------------------

            if (distanceToPlayer <=
                attackDistance)
            {
                TryAttack();
            }
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


            StartCoroutine(
                PerformAttack()
            );
        }


        // =========================================================
        // CORRUTINA DEL ATAQUE
        // =========================================================

        private IEnumerator PerformAttack()
        {
            isPerformingAttack = true;


            // -----------------------------------------------------
            // COOLDOWN
            // -----------------------------------------------------

            attackTimer =
                attackCooldown;


            // -----------------------------------------------------
            // DETENER A MUKI
            // -----------------------------------------------------

            if (agent != null)
            {
                agent.isStopped = true;

                agent.ResetPath();
            }


            // -----------------------------------------------------
            // ESTADO DE PERSECUCIÓN SE MANTIENE
            // -----------------------------------------------------

            currentState =
                EnemyState.Chasing;


            // -----------------------------------------------------
            // ANIMACIÓN DE ATAQUE
            // -----------------------------------------------------

            if (animator != null)
            {
                animator.ResetTrigger("Attack");

                animator.SetTrigger("Attack");
            }


            Debug.Log(
                "¡Muki atrapó al jugador! " +
                "Reproduciendo animación de ataque."
            );


            // -----------------------------------------------------
            // ESPERAR LA ANIMACIÓN
            // -----------------------------------------------------

            yield return new WaitForSeconds(
                attackAnimationDuration
            );


            // =====================================================
            // REGRESAR AL CHECKPOINT
            // =====================================================

            if (PlayerRespawn.Instance != null)
            {
                PlayerRespawn.Instance.Respawn();
            }
            else
            {
                Debug.LogWarning(
                    "Muki intentó matar al jugador, " +
                    "pero no existe PlayerRespawn.Instance."
                );
            }


            // =====================================================
            // ACTIVAR PROTECCIÓN
            // =====================================================

            postKillProtectionTimer =
                postKillProtectionTime;


            // =====================================================
            // LIMPIAR ESTADO ANTERIOR
            // =====================================================

            investigatingNoise =
                false;

            noisePatrolPoint =
                null;

            waitingAtNoisePoint =
                false;

            noiseWaitTimer =
                0f;

            calmPatrolTarget =
                null;

            patrolTimer =
                0f;


            // =====================================================
            // BUSCAR PUNTO LEJANO
            // =====================================================

            Transform safePatrolPoint =
                GetSafePostKillPatrolPoint();


            // =====================================================
            // MOVER A MUKI
            // =====================================================

            if (safePatrolPoint != null)
            {
                MoveMukiToPatrolPoint(
                    safePatrolPoint
                );
            }
            else
            {
                currentState =
                    EnemyState.Patrolling;

                agent.speed =
                    patrolSpeed;

                agent.isStopped =
                    false;

                GoToNextPatrolPoint();
            }


            isPerformingAttack =
                false;


            Debug.Log(
                "Muki se alejó después de matar al jugador. " +
                "No podrá detectarlo durante " +
                postKillProtectionTime +
                " segundos."
            );
        }


        // =========================================================
        // MOVER MUKI A UN PUNTO LEJANO
        // =========================================================

        private void MoveMukiToPatrolPoint(
            Transform target)
        {
            if (target == null)
                return;


            NavMeshHit navHit;


            // -----------------------------------------------------
            // BUSCAR POSICIÓN VÁLIDA EN NAVMESH
            // -----------------------------------------------------

            if (NavMesh.SamplePosition(
                target.position,
                out navHit,
                3f,
                NavMesh.AllAreas))
            {
                agent.Warp(
                    navHit.position
                );
            }


            // -----------------------------------------------------
            // ACTUALIZAR ÍNDICE
            // -----------------------------------------------------

            currentPatrolIndex =
                GetPatrolPointIndex(
                    target
                );


            // -----------------------------------------------------
            // VOLVER A PATRULLAR
            // -----------------------------------------------------

            currentState =
                EnemyState.Patrolling;

            agent.speed =
                patrolSpeed;

            agent.isStopped =
                false;

            agent.ResetPath();


            // Continuar hacia el siguiente punto.
            GoToNextPatrolPoint();
        }


        // =========================================================
        // OBTENER ÍNDICE DEL PUNTO
        // =========================================================

        private int GetPatrolPointIndex(
            Transform target)
        {
            if (patrolPoints == null)
                return 0;


            for (
                int i = 0;
                i < patrolPoints.Length;
                i++)
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
            if (patrolPoints == null ||
                patrolPoints.Length == 0)
            {
                return null;
            }


            Transform safePoint =
                null;

            float bestDistance =
                -1f;


            // -----------------------------------------------------
            // PRIMERO: BUSCAR UNO QUE ESTÉ REALMENTE LEJOS
            // -----------------------------------------------------

            foreach (
                Transform point
                in patrolPoints)
            {
                if (point == null)
                    continue;


                float distance =
                    Vector3.Distance(
                        point.position,
                        player.position
                    );


                if (distance >=
                    postKillMinDistance &&
                    distance > bestDistance)
                {
                    bestDistance =
                        distance;

                    safePoint =
                        point;
                }
            }


            if (safePoint != null)
                return safePoint;


            // -----------------------------------------------------
            // SI NINGUNO CUMPLE, USAR EL MÁS LEJANO
            // -----------------------------------------------------

            return GetFarthestPatrolPoint();
        }


        // =========================================================
        // COMENZAR BÚSQUEDA
        // =========================================================

        private void StartSearching()
        {
            currentState =
                EnemyState.Searching;

            searchTimer =
                searchTime;

            agent.speed =
                patrolSpeed;

            agent.isStopped =
                false;

            agent.ResetPath();


            Debug.Log(
                "Muki está buscando al jugador."
            );
        }


        // =========================================================
        // BUSCAR
        // =========================================================

        private void Search()
        {
            // -----------------------------------------------------
            // DETECTAR NUEVAMENTE
            // -----------------------------------------------------

            if (postKillProtectionTimer <= 0f)
            {
                if (IsPlayerDetectable())
                {
                    float distanceToPlayer =
                        Vector3.Distance(
                            transform.position,
                            player.position
                        );


                    if (distanceToPlayer <=
                        detectionRange)
                    {
                        StartChasing();

                        return;
                    }
                }
            }


            // -----------------------------------------------------
            // TEMPORIZADOR
            // -----------------------------------------------------

            searchTimer -=
                Time.deltaTime;


            if (searchTimer <= 0f)
            {
                currentState =
                    EnemyState.Patrolling;

                agent.speed =
                    patrolSpeed;

                patrolTimer =
                    0f;

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
            // -----------------------------------------------------
            // PROTECCIÓN
            // -----------------------------------------------------

            if (postKillProtectionTimer > 0f)
                return;


            // -----------------------------------------------------
            // ATAQUE
            // -----------------------------------------------------

            if (isPerformingAttack)
                return;


            // -----------------------------------------------------
            // ALTAR
            // -----------------------------------------------------

            if (IsAnyAltarActive())
                return;


            // -----------------------------------------------------
            // ZONA SEGURA
            // -----------------------------------------------------

            if (IsPlayerInsideSafeZone())
                return;


            // -----------------------------------------------------
            // YA PERSIGUE
            // -----------------------------------------------------

            if (currentState ==
                EnemyState.Chasing)
            {
                return;
            }


            // -----------------------------------------------------
            // YA INVESTIGA
            // -----------------------------------------------------

            if (currentState ==
                EnemyState.InvestigatingNoise)
            {
                return;
            }


            // -----------------------------------------------------
            // DISTANCIA AL RUIDO
            // -----------------------------------------------------

            float distanceToNoise =
                Vector3.Distance(
                    transform.position,
                    noisePosition
                );


            if (distanceToNoise >
                noiseRadius)
            {
                return;
            }


            // -----------------------------------------------------
            // PUNTO MÁS CERCANO
            // -----------------------------------------------------

            Transform closestPoint =
                null;

            float closestDistance =
                Mathf.Infinity;


            if (patrolPoints != null)
            {
                foreach (
                    Transform point
                    in patrolPoints)
                {
                    if (point == null)
                        continue;


                    float distance =
                        Vector3.Distance(
                            point.position,
                            noisePosition
                        );


                    if (distance <=
                        noiseRadius &&
                        distance <
                        closestDistance)
                    {
                        closestDistance =
                            distance;

                        closestPoint =
                            point;
                    }
                }
            }


            if (closestPoint == null)
                return;


            // -----------------------------------------------------
            // COMENZAR INVESTIGACIÓN
            // -----------------------------------------------------

            investigatingNoise =
                true;

            waitingAtNoisePoint =
                false;

            noiseWaitTimer =
                0f;

            noisePatrolPoint =
                closestPoint;

            currentState =
                EnemyState.InvestigatingNoise;

            agent.speed =
                patrolSpeed;

            agent.isStopped =
                false;

            agent.SetDestination(
                closestPoint.position
            );


            Debug.Log(
                "Muki escuchó un ruido: " +
                noiseType
            );
        }


        // =========================================================
        // INVESTIGAR RUIDO
        // =========================================================

        private void InvestigateNoise()
        {
            // -----------------------------------------------------
            // ESCONDIDO
            // -----------------------------------------------------

            if (playerHide != null &&
                playerHide.IsHidden)
            {
                return;
            }


            // -----------------------------------------------------
            // ZONA SEGURA
            // -----------------------------------------------------

            if (IsPlayerInsideSafeZone())
            {
                investigatingNoise =
                    false;

                waitingAtNoisePoint =
                    false;

                noisePatrolPoint =
                    null;

                currentState =
                    EnemyState.Patrolling;

                agent.speed =
                    patrolSpeed;

                agent.isStopped =
                    false;

                GoToNextPatrolPoint();

                return;
            }


            // -----------------------------------------------------
            // DETECTAR JUGADOR
            // -----------------------------------------------------

            if (!waitingAtNoisePoint &&
                IsPlayerDetectable())
            {
                float distanceToPlayer =
                    Vector3.Distance(
                        transform.position,
                        player.position
                    );


                if (distanceToPlayer <=
                    detectionRange)
                {
                    investigatingNoise =
                        false;

                    noisePatrolPoint =
                        null;

                    StartChasing();

                    return;
                }
            }


            // -----------------------------------------------------
            // SIN DESTINO
            // -----------------------------------------------------

            if (noisePatrolPoint == null)
            {
                investigatingNoise =
                    false;

                waitingAtNoisePoint =
                    false;

                currentState =
                    EnemyState.Patrolling;

                agent.isStopped =
                    false;

                GoToNextPatrolPoint();

                return;
            }


            // =====================================================
            // YA ESTÁ ESPERANDO EN EL PUNTO
            // =====================================================

            if (waitingAtNoisePoint)
            {
                agent.isStopped =
                    true;

                noiseWaitTimer -=
                    Time.deltaTime;


                if (noiseWaitTimer <= 0f)
                {
                    waitingAtNoisePoint =
                        false;

                    investigatingNoise =
                        false;

                    noisePatrolPoint =
                        null;

                    currentState =
                        EnemyState.Patrolling;

                    patrolTimer =
                        0f;

                    agent.isStopped =
                        false;

                    GoToNextPatrolPoint();

                    Debug.Log(
                        "Muki terminó de investigar " +
                        "y continúa patrullando."
                    );
                }

                return;
            }


            // =====================================================
            // LLEGÓ AL PUNTO DEL RUIDO
            // =====================================================

            if (!agent.pathPending &&
                agent.remainingDistance <=
                agent.stoppingDistance)
            {
                Debug.Log(
                    "Muki llegó al lugar del ruido. " +
                    "Ahora esperará."
                );


                // -------------------------------------------------
                // DETENER MUKI
                // -------------------------------------------------

                agent.isStopped =
                    true;


                agent.ResetPath();


                // -------------------------------------------------
                // ACTIVAR ESPERA
                // -------------------------------------------------

                waitingAtNoisePoint =
                    true;

                noiseWaitTimer =
                    investigationWaitTime;


                // -------------------------------------------------
                // IMPORTANTE:
                // IsWalking pasa automáticamente a FALSE
                // y el Animator reproduce IDLE.
                // -------------------------------------------------

                return;
            }
        }


        // =========================================================
        // JUGADOR DETECTABLE
        // =========================================================

        private bool IsPlayerDetectable()
        {
            // -----------------------------------------------------
            // PROTECCIÓN
            // -----------------------------------------------------

            if (postKillProtectionTimer > 0f)
                return false;


            // -----------------------------------------------------
            // ATAQUE
            // -----------------------------------------------------

            if (isPerformingAttack)
                return false;


            // -----------------------------------------------------
            // ALTAR
            // -----------------------------------------------------

            if (IsAnyAltarActive())
                return false;


            // -----------------------------------------------------
            // ESCONDIDO
            // -----------------------------------------------------

            if (playerHide != null &&
                playerHide.IsHidden)
            {
                return false;
            }


            // -----------------------------------------------------
            // ZONA SEGURA
            // -----------------------------------------------------

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


            foreach (
                MukiSafeZone zone
                in safeZones)
            {
                if (zone == null)
                    continue;


                if (zone.Contains(
                    player.position))
                {
                    return true;
                }
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


            foreach (
                OfferingAltar altar
                in altars)
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
            currentState =
                EnemyState.Calmed;

            agent.speed =
                patrolSpeed;

            agent.isStopped =
                false;

            agent.ResetPath();


            calmPatrolTarget =
                GetFarthestPatrolPoint();


            if (calmPatrolTarget != null)
            {
                agent.SetDestination(
                    calmPatrolTarget.position
                );
            }


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
            agent.speed =
                patrolSpeed;

            agent.isStopped =
                false;


            if (calmPatrolTarget == null)
            {
                calmPatrolTarget =
                    GetFarthestPatrolPoint();


                if (calmPatrolTarget != null)
                {
                    agent.SetDestination(
                        calmPatrolTarget.position
                    );
                }


                return;
            }


            if (!agent.pathPending &&
                agent.remainingDistance <=
                agent.stoppingDistance)
            {
                calmPatrolTarget =
                    GetFarthestPatrolPoint();


                if (calmPatrolTarget != null)
                {
                    agent.SetDestination(
                        calmPatrolTarget.position
                    );
                }
            }
        }


        // =========================================================
        // PUNTO MÁS LEJANO DEL JUGADOR
        // =========================================================

        private Transform GetFarthestPatrolPoint()
        {
            if (patrolPoints == null ||
                patrolPoints.Length == 0)
            {
                return null;
            }


            Transform farthestPoint =
                null;

            float greatestDistance =
                -1f;


            foreach (
                Transform point
                in patrolPoints)
            {
                if (point == null)
                    continue;


                if (point ==
                    calmPatrolTarget &&
                    patrolPoints.Length > 1)
                {
                    continue;
                }


                float distance = 0f;


                if (player != null)
                {
                    distance =
                        Vector3.Distance(
                            point.position,
                            player.position
                        );
                }


                if (distance >
                    greatestDistance)
                {
                    greatestDistance =
                        distance;

                    farthestPoint =
                        point;
                }
            }


            if (farthestPoint == null)
            {
                farthestPoint =
                    calmPatrolTarget;
            }


            return farthestPoint;
        }


        // =========================================================
        // TERMINAR CALMADO
        // =========================================================

        private void EndCalmed()
        {
            currentState =
                EnemyState.Patrolling;

            agent.speed =
                patrolSpeed;

            agent.isStopped =
                false;

            calmPatrolTarget =
                null;

            patrolTimer =
                0f;

            GoToNextPatrolPoint();


            Debug.Log(
                "El efecto del altar terminó. " +
                "Muki vuelve a patrullar normalmente."
            );
        }


        // =========================================================
        // AUDIO
        // =========================================================

        private void UpdateStateAudio()
        {
            // -----------------------------------------------------
            // No hacer nada si el estado no cambió
            // -----------------------------------------------------

            if (audioStateInitialized &&
                lastAudioState ==
                currentState)
            {
                return;
            }


            lastAudioState =
                currentState;

            audioStateInitialized =
                true;


            // -----------------------------------------------------
            // DETENER LOOP ANTERIOR
            // -----------------------------------------------------

            if (stateLoopAudio != null)
                stateLoopAudio.Stop();


            // =====================================================
            // INVESTIGACIÓN
            // =====================================================

            if (currentState ==
                EnemyState.InvestigatingNoise)
            {
                if (investigationStartSound != null)
                {
                    oneShotAudio.PlayOneShot(
                        investigationStartSound
                    );
                }


                if (investigationLoopSound != null)
                {
                    stateLoopAudio.clip =
                        investigationLoopSound;

                    stateLoopAudio.loop =
                        true;

                    stateLoopAudio.Play();
                }
            }


            // =====================================================
            // PERSECUCIÓN
            // =====================================================

            else if (currentState ==
                     EnemyState.Chasing)
            {
                if (chaseLaughSound != null)
                {
                    stateLoopAudio.clip =
                        chaseLaughSound;

                    stateLoopAudio.loop =
                        true;

                    stateLoopAudio.Play();
                }
            }


            // =====================================================
            // OTROS ESTADOS
            // =====================================================

            else
            {
                // El loop ya fue detenido arriba.
            }
        }
    }
}
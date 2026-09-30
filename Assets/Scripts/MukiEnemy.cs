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


        // =========================================================
        // INVESTIGACIÓN
        // =========================================================

        [Header("Investigación de ruido")]
        public float searchTime = 3f;

        private float searchTimer = 0f;

        private bool investigatingNoise = false;

        private Transform noisePatrolPoint;


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
            // DETECTAR JUGADOR
            // -----------------------------------------------------

            if (IsPlayerDetectable())
            {
                float distanceToPlayer =
                    Vector3.Distance(
                        transform.position,
                        player.position
                    );


                if (distanceToPlayer <= detectionRange)
                {
                    StartChasing();

                    return;
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


            attackTimer =
                attackCooldown;


            Debug.Log(
                "¡Muki atrapó al jugador!"
            );


            // =====================================================
            // REGRESAR AL ÚLTIMO CHECKPOINT
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
            // REINICIAR COMPORTAMIENTO DEL MUKI
            // =====================================================

            currentState =
                EnemyState.Patrolling;

            investigatingNoise =
                false;

            noisePatrolPoint =
                null;

            calmPatrolTarget =
                null;

            patrolTimer =
                0f;

            agent.speed =
                patrolSpeed;

            agent.isStopped =
                false;

            agent.ResetPath();

            GoToNextPatrolPoint();
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

            Transform closestPoint = null;

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

                noisePatrolPoint =
                    null;

                currentState =
                    EnemyState.Patrolling;

                agent.speed =
                    patrolSpeed;

                GoToNextPatrolPoint();

                return;
            }


            // -----------------------------------------------------
            // DETECTAR JUGADOR
            // -----------------------------------------------------

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

                currentState =
                    EnemyState.Patrolling;

                GoToNextPatrolPoint();

                return;
            }


            // -----------------------------------------------------
            // LLEGÓ AL RUIDO
            // -----------------------------------------------------

            if (!agent.pathPending &&
                agent.remainingDistance <=
                agent.stoppingDistance)
            {
                Debug.Log(
                    "Muki llegó al lugar del ruido."
                );


                investigatingNoise =
                    false;

                noisePatrolPoint =
                    null;

                currentState =
                    EnemyState.Patrolling;

                patrolTimer =
                    0f;

                GoToNextPatrolPoint();
            }
        }


        // =========================================================
        // JUGADOR DETECTABLE
        // =========================================================

        private bool IsPlayerDetectable()
        {
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
                // Sonido que ocurre una sola vez

                if (investigationStartSound != null)
                {
                    oneShotAudio.PlayOneShot(
                        investigationStartSound
                    );
                }


                // Loop mientras investiga

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
                //
                // El sonido de inicio de investigación
                // NO se corta y puede terminar naturalmente.
            }
        }
    }
}
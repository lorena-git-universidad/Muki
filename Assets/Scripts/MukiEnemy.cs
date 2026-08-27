using UnityEngine;
using UnityEngine.AI;

namespace StarterAssets
{
    public class MukiEnemy : MonoBehaviour
    {
        public enum EnemyState
        {
            Patrolling,
            Chasing,
            Searching,
            Calmed,
            InvestigatingNoise,
        }

        [Header("References")]
        public Transform player;
        public PlayerHide playerHide;

        [Header("Altars")]
        public OfferingAltar[] altars;

        [Header("Calmed State")]
        public bool stopCompletelyWhenCalmed = true;
        public float calmedSpeed = 0.5f;

        [Header("Patrol")]
        public Transform[] patrolPoints;
        public float patrolWaitTime = 2f;

        [Header("Detection")]
        public float detectionRange = 12f;
        public float losePlayerRange = 18f;

        [Header("Chase")]
        public float patrolSpeed = 2.5f;
        public float chaseSpeed = 5f;

        [Header("Search")]
        public float searchTime = 3f;

        [Header("Attack")]
        public float attackDistance = 1.5f;
        public float attackCooldown = 1f;

        private float attackTimer;

        [Header("State")]
        public EnemyState currentState;

        private NavMeshAgent agent;

        private int currentPatrolPoint;
        private float patrolTimer;
        private float searchTimer;
        private bool investigatingNoise;
        private Transform noisePatrolPoint;

        public void HearNoise(
        Vector3 noisePosition,
        float noiseRadius,
        string noiseType)
        {
            if (IsAnyAltarActive())
            {
                Debug.Log(
                    "Muki ignoró el ruido porque hay un altar activo."
                );

                return;
            }

            if (playerHide != null &&
                playerHide.IsHidden)
            {
                Debug.Log(
                    "Muki ignoró el ruido porque el jugador está escondido."
                );

                return;
            }

            Transform patrolPoint =
                GetPatrolPointInsideNoise(
                    noisePosition,
                    noiseRadius
                );

            if (patrolPoint == null)
            {
                Debug.Log(
                    $"Muki escuchó {noiseType}, " +
                    $"pero no hay patrol points dentro del radio."
                );

                return;
            }

            investigatingNoise = true;

            currentState = EnemyState.Patrolling;

            agent.speed = patrolSpeed;

            agent.SetDestination(
                patrolPoint.position
            );

            Debug.Log(
                $"<color=red>MUKI ESCUCHÓ:</color> " +
                $"{noiseType} ? " +
                $"Investigando {patrolPoint.name}"
            );
        }

        private Transform GetPatrolPointInsideNoise(
    Vector3 noisePosition,
    float noiseRadius)
        {
            if (patrolPoints == null ||
                patrolPoints.Length == 0)
                return null;

            Transform[] validPoints =
                new Transform[patrolPoints.Length];

            int validCount = 0;

            foreach (Transform point in patrolPoints)
            {
                if (point == null)
                    continue;

                float distance =
                    Vector3.Distance(
                        point.position,
                        noisePosition);

                if (distance <= noiseRadius)
                {
                    validPoints[validCount] = point;
                    validCount++;
                }
            }

            if (validCount == 0)
                return null;

            // Elegir uno aleatoriamente
            int randomIndex =
                Random.Range(0, validCount);

            return validPoints[randomIndex];
        }
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();

            // Buscar jugador automáticamente
            if (player == null)
            {
                GameObject playerObject =
                    GameObject.FindGameObjectWithTag("Player");

                if (playerObject != null)
                    player = playerObject.transform;
            }

            // Buscar PlayerHide
            if (player != null && playerHide == null)
            {
                playerHide =
                    player.GetComponent<PlayerHide>();
            }

            // Buscar altares automáticamente si no los asignamos
            if (altars == null || altars.Length == 0)
            {
                altars =
                    FindObjectsByType<OfferingAltar>(
                        FindObjectsSortMode.None);
            }
        }

        private void Start()
        {
            currentState = EnemyState.Patrolling;

            agent.speed = patrolSpeed;

            GoToNextPatrolPoint();
        }

        private void Update()
        {


            if (player == null)
                return;

            // ==========================================
            // ALTAR ACTIVO
            // ==========================================

            if (IsAnyAltarActive())
            {
                if (currentState != EnemyState.Calmed)
                {
                    StartCalmed();
                }

                Calm();
                return;
            }

            // ==========================================
            // COMPORTAMIENTO NORMAL
            // ==========================================

            // Si estaba calmado y el altar terminó
            if (currentState == EnemyState.Calmed)
            {
                EndCalmed();
            }

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
                    Calm();
                    break;

                case EnemyState.InvestigatingNoise:
                    InvestigateNoise();
                    break;
            }
        }

        private void InvestigateNoise()
        {
            if (IsAnyAltarActive())
            {
                StartCalmed();
                return;
            }

            if (IsPlayerDetectable())
            {
                float distance =
                    Vector3.Distance(
                        transform.position,
                        player.position
                    );

                if (distance <= detectionRange)
                {
                    StartChasing();
                    return;
                }
            }

            if (noisePatrolPoint == null)
            {
                currentState = EnemyState.Patrolling;
                GoToNextPatrolPoint();
                return;
            }

            if (!agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance)
            {
                Debug.Log(
                    "Muki llegó al lugar donde escuchó el ruido."
                );

                noisePatrolPoint = null;

                currentState = EnemyState.Patrolling;

                patrolTimer = 0f;

                GoToNextPatrolPoint();
            }
        }

        // =====================================================
        // ALTAR
        // =====================================================

        private bool IsAnyAltarActive()
        {
            if (altars == null || altars.Length == 0)
                return false;

            foreach (OfferingAltar altar in altars)
            {
                if (altar != null && altar.isActive)
                    return true;
            }

            return false;
        }

        private void StartCalmed()
        {
            currentState = EnemyState.Calmed;

            // Detener persecución o movimiento actual
            agent.ResetPath();

            if (stopCompletelyWhenCalmed)
            {
                agent.speed = 0f;
            }
            else
            {
                agent.speed = calmedSpeed;
            }

            Debug.Log(
                "Muki está calmado. No puede detectar ni perseguir al jugador."
            );
        }

        private void Calm()
        {
            // Mientras haya un altar activo,
            // el Muki permanece calmado.

            if (stopCompletelyWhenCalmed)
            {
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
        }

        private void EndCalmed()
        {
            currentState = EnemyState.Patrolling;

            agent.speed = patrolSpeed;

            patrolTimer = 0f;

            GoToNextPatrolPoint();

            Debug.Log(
                "El efecto del altar terminó. Muki vuelve a patrullar."
            );
        }

        // =====================================================
        // PATROL
        // =====================================================

        private void Patrol()
        {
            if (IsPlayerDetectable())
            {
                float distance =
                    Vector3.Distance(
                        transform.position,
                        player.position);

                if (distance <= detectionRange)
                {
                    StartChasing();
                    return;
                }
            }

            if (patrolPoints == null ||
                patrolPoints.Length == 0)
            {
                return;
            }

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

        private void GoToNextPatrolPoint()
        {
            if (patrolPoints == null ||
                patrolPoints.Length == 0)
                return;

            Transform target =
                patrolPoints[currentPatrolPoint];

            if (target != null)
            {
                agent.SetDestination(target.position);
            }

            currentPatrolPoint++;

            if (currentPatrolPoint >= patrolPoints.Length)
                currentPatrolPoint = 0;
        }

        // =====================================================
        // CHASE
        // =====================================================

        private void StartChasing()
        {
            // Seguridad adicional:
            // nunca iniciar persecución si un altar está activo.
            if (IsAnyAltarActive())
                return;

            currentState = EnemyState.Chasing;

            agent.speed = chaseSpeed;

            Debug.Log(
                "Muki comenzó a perseguir al jugador."
            );
        }

        private void ChasePlayer()
        {
            if (IsAnyAltarActive())
            {
                StartCalmed();
                return;
            }

            if (playerHide != null &&
                playerHide.IsHidden)
            {
                StartSearching();
                return;
            }

            float distance =
                Vector3.Distance(
                    transform.position,
                    player.position);

            // =========================
            // ATAQUE
            // =========================

            if (distance <= attackDistance)
            {
                TryAttackPlayer();
                return;
            }

            // =========================
            // PERDER AL JUGADOR
            // =========================

            if (distance > losePlayerRange)
            {
                StartSearching();
                return;
            }

            agent.SetDestination(player.position);
        }

        // =========================
        // INTENTAR ATACAR AL JUGADOR
        // =========================

        private void TryAttackPlayer()
        {
            if (playerHide != null &&
                playerHide.IsHidden)
            {
                return;
            }

            attackTimer -= Time.deltaTime;

            if (attackTimer > 0f)
                return;

            attackTimer = attackCooldown;

            Debug.Log("¡Muki atrapó al jugador!");

            if (PlayerRespawn.Instance != null)
            {
                PlayerRespawn.Instance.Respawn();
            }
        }

        // =====================================================
        // SEARCH
        // =====================================================

        private void StartSearching()
        {
            currentState = EnemyState.Searching;

            searchTimer = searchTime;

            agent.speed = patrolSpeed;

            agent.ResetPath();

            Debug.Log(
                "Muki perdió al jugador. Buscando..."
            );
        }

        private void Search()
        {
            // Si se activa un altar mientras busca
            if (IsAnyAltarActive())
            {
                StartCalmed();
                return;
            }

            searchTimer -= Time.deltaTime;

            if (IsPlayerDetectable())
            {
                float distance =
                    Vector3.Distance(
                        transform.position,
                        player.position);

                if (distance <= detectionRange)
                {
                    StartChasing();
                    return;
                }
            }

            if (searchTimer <= 0f)
            {
                currentState =
                    EnemyState.Patrolling;

                GoToNextPatrolPoint();

                Debug.Log(
                    "Muki dejó de buscar y volvió a patrullar."
                );
            }
        }

        // =====================================================
        // DETECTION
        // =====================================================

        private bool IsPlayerDetectable()
        {
            // Altar activo = jugador invisible para el Muki
            if (IsAnyAltarActive())
                return false;

            // Escondite = jugador invisible
            if (playerHide != null &&
                playerHide.IsHidden)
            {
                return false;
            }

            return true;
        }


    }


}
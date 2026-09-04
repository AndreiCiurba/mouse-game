using MouseGame.Game;
using UnityEngine;
using UnityEngine.AI;

namespace MouseGame.AI
{
    /// <summary>
    /// Idle -> Patrol -> (sees/hears player) -> Chase -> (loses player) -> Search ->
    /// (times out) -> Patrol, or (catches player) -> Game Over. Seeing the player always wins
    /// and jumps straight to Chase from any state.
    ///
    /// Each state is a private method rather than a separate class/object — the simplest version
    /// that still keeps states clearly separated, per the README's "keep AI modular" guidance.
    /// Promote to a real State-object pattern if/when more states make this switch unwieldy.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CatVision))]
    [RequireComponent(typeof(CatHearing))]
    public class CatAI : MonoBehaviour
    {
        private enum State { Idle, Patrol, InvestigateNoise, Chase, Search }

        [Header("Speeds")]
        [SerializeField] private float patrolSpeed = 0.6f;
        [SerializeField] private float chaseSpeed = 1.4f;

        [Header("Idle / Patrol")]
        [SerializeField] private float idleDuration = 2f;
        [SerializeField] private float patrolRadius = 2f;
        [SerializeField] private float waypointTolerance = 0.15f;

        [Header("Search")]
        [Tooltip("How long to linger at the last known position before giving up and patrolling again.")]
        [SerializeField] private float searchDuration = 4f;
        [Tooltip("How long Chase keeps heading to the last-seen spot after losing sight before giving up to Search.")]
        [SerializeField] private float loseSightGrace = 1f;

        [Header("Catch")]
        [SerializeField] private float catchDistance = 0.15f;

        [Header("References")]
        [SerializeField] private GameOverManager gameOverManager;

        private NavMeshAgent agent;
        private CatVision vision;
        private CatHearing hearing;
        private State state;
        private float stateTimer;
        private Vector3 investigatePoint;
        private Vector3 spawnPosition;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            vision = GetComponent<CatVision>();
            hearing = GetComponent<CatHearing>();
            spawnPosition = transform.position;
        }

        private void Start()
        {
            EnterState(State.Idle);
        }

        private void Update()
        {
            if (state != State.Chase && vision.CanSeePlayer(out Vector3 seenPos))
            {
                investigatePoint = seenPos;
                EnterState(State.Chase);
            }

            switch (state)
            {
                case State.Idle: UpdateIdle(); break;
                case State.Patrol: UpdatePatrol(); break;
                case State.InvestigateNoise: UpdateInvestigateNoise(); break;
                case State.Chase: UpdateChase(); break;
                case State.Search: UpdateSearch(); break;
            }
        }

        private void EnterState(State next)
        {
            state = next;
            stateTimer = 0f;

            switch (next)
            {
                case State.Idle:
                    agent.isStopped = true;
                    break;
                case State.Patrol:
                    agent.isStopped = false;
                    agent.speed = patrolSpeed;
                    SetRandomPatrolDestination();
                    break;
                case State.InvestigateNoise:
                    agent.isStopped = false;
                    agent.speed = patrolSpeed;
                    agent.SetDestination(investigatePoint);
                    break;
                case State.Chase:
                    agent.isStopped = false;
                    agent.speed = chaseSpeed;
                    break;
                case State.Search:
                    agent.isStopped = false;
                    agent.speed = patrolSpeed;
                    agent.SetDestination(investigatePoint);
                    break;
            }
        }

        private void UpdateIdle()
        {
            stateTimer += Time.deltaTime;
            if (stateTimer >= idleDuration)
            {
                EnterState(State.Patrol);
                return;
            }

            CheckHearing();
        }

        private void UpdatePatrol()
        {
            if (HasReachedDestination())
            {
                EnterState(State.Idle);
                return;
            }

            CheckHearing();
        }

        private void UpdateInvestigateNoise()
        {
            if (HasReachedDestination())
            {
                EnterState(State.Search);
            }
        }

        private void UpdateChase()
        {
            if (vision.CanSeePlayer(out Vector3 seenPos))
            {
                investigatePoint = seenPos;
                agent.SetDestination(seenPos);
                stateTimer = 0f;

                if (Vector3.Distance(transform.position, seenPos) <= catchDistance)
                {
                    gameOverManager?.PlayerCaught();
                    agent.isStopped = true;
                    enabled = false; // stop reacting once the game is over
                }

                return;
            }

            // Lost sight - keep heading to the last known spot briefly, then give up to Search.
            stateTimer += Time.deltaTime;
            if (stateTimer > loseSightGrace || HasReachedDestination())
            {
                EnterState(State.Search);
            }
        }

        private void UpdateSearch()
        {
            stateTimer += Time.deltaTime;

            if (stateTimer >= searchDuration)
            {
                EnterState(State.Patrol);
                return;
            }

            CheckHearing();
        }

        private void CheckHearing()
        {
            if (hearing.CanHearPlayer(out Vector3 heardPos))
            {
                investigatePoint = heardPos;
                EnterState(State.InvestigateNoise);
            }
        }

        private void SetRandomPatrolDestination()
        {
            Vector3 randomOffset = Random.insideUnitSphere * patrolRadius;
            randomOffset.y = 0f;
            Vector3 candidate = spawnPosition + randomOffset;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        private bool HasReachedDestination()
        {
            if (agent.pathPending || !agent.hasPath)
            {
                return false;
            }

            return agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, waypointTolerance);
        }
    }
}

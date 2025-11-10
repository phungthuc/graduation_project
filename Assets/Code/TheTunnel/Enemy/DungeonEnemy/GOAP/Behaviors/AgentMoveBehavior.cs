using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Interfaces;
using UnityEngine;
using UnityEngine.AI;

namespace TheTunnel.GOAP
{
    [RequireComponent(typeof(NavMeshAgent), typeof(AgentBehaviour))]
    public class AgentMoveBehavior : MonoBehaviour
    {
        private NavMeshAgent _navMeshAgent;
        private AgentBehaviour _agentBehavior;
        private ITarget _currentTarget;

        [SerializeField] private float minMoveDistance = 0.25f;

        private Vector3 _lastPosition;
        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _agentBehavior = GetComponent<AgentBehaviour>();
        }

        private void OnEnable()
        {
            _agentBehavior.Events.OnTargetChanged += EventsOnTargetChanged;
            _agentBehavior.Events.OnTargetOutOfRange += EventsOnTargetOutOfRange;
        }

        private void OnDisable()
        {
            _agentBehavior.Events.OnTargetChanged -= EventsOnTargetChanged;
            _agentBehavior.Events.OnTargetOutOfRange -= EventsOnTargetOutOfRange;
        }

        private void EventsOnTargetOutOfRange(ITarget target)
        {
        }

        private void EventsOnTargetChanged(ITarget target, bool inRange)
        {
            _currentTarget = target;
            _lastPosition = _currentTarget.Position;
            if (_navMeshAgent.isActiveAndEnabled)
                _navMeshAgent.SetDestination(target.Position);
        }

        private void Update()
        {
            if (_currentTarget == null)
            {
                return;
            }

            if (minMoveDistance <= Vector3.Distance(_currentTarget.Position, _lastPosition))
            {
                _lastPosition = _currentTarget.Position;
                if (_navMeshAgent.isActiveAndEnabled)
                    _navMeshAgent.SetDestination(_currentTarget.Position);
            }
        }
    }
}
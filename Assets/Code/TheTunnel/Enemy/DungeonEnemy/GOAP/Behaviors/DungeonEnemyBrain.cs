using System;
using System.Collections;
using cowsins;
using CrashKonijn.Goap.Behaviours;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace TheTunnel.GOAP
{
    [RequireComponent(typeof(AgentBehaviour))]
    public class DungeonEnemyBrain : MonoBehaviour
    {
        [SerializeField] private PlayerSensor playerSensor;
        [SerializeField] private AttackConfigSO attackConfigSo;

        private EnemyHealth _enemyHealth;
        private AgentBehaviour _agentBehavior;
        private Coroutine _exitCoroutine;
        private bool _isPlayerInRange;
        private GoapSetBinder _goapBinder;
        private void Awake()
        {
            _agentBehavior = GetComponent<AgentBehaviour>();
            _enemyHealth = GetComponent<EnemyHealth>();
            _goapBinder = GetComponent<GoapSetBinder>();
        }

        private void Start()
        {
            playerSensor.collider.radius = attackConfigSo.sensorRadius;
            _enemyHealth.events.OnDamaged.AddListener(OnDamaged);
            DetermineGoal();
        }

        private void DetermineGoal()
        {
            switch (_goapBinder.agentType)
            {
                case AgentType.EnemyDungeonSet:
                case AgentType.RangedEnemyDungeonSet:
                case AgentType.SpiderEnemyDungeonSet:
                    _agentBehavior.SetGoal<PatrolGoal>(false);
                    playerSensor.OnPlayerEnter += PlayerSensorOnPlayerEnter;
                    playerSensor.OnPlayerExit += PlayerSensorOnPlayerExit;
                    break;
                case AgentType.WormEnemyDungeonSet:
                    _agentBehavior.SetGoal<HiddenGoal>(false);
                    playerSensor.OnPlayerEnter += HandleWormPlayerEnterSensor;
                    playerSensor.OnPlayerExit += HandleWormPlayerExitSensor;
                    break;
            }
        }

        private void PlayerSensorOnPlayerEnter(Transform player)
        {
            _isPlayerInRange = true;

            if (_exitCoroutine != null)
            {
                StopCoroutine(_exitCoroutine);
                _exitCoroutine = null;
            }
            playerSensor.collider.radius = 40;
            _agentBehavior.SetGoal<PatrolGoal>(false);
            _agentBehavior.SetGoal<KillPlayerGoal>(true);
        }

        private void PlayerSensorOnPlayerExit(Vector3 lastKnownPosition)
        {
            _isPlayerInRange = false;

            if (_exitCoroutine == null)
            {
                _exitCoroutine = StartCoroutine(WaitAndReturnToPatrol());
            }
        }

        #region Worm
        private void HandleWormPlayerEnterSensor(Transform player)
        {
            _isPlayerInRange = true;
            playerSensor.collider.radius = 10;
            _agentBehavior.SetGoal<HiddenGoal>(false);
            _agentBehavior.SetGoal<KillPlayerGoal>(true);
        }

        private void HandleWormPlayerExitSensor(Vector3 lastKnownPosition)
        {
            _isPlayerInRange = false;
            _agentBehavior.SetGoal<KillPlayerGoal>(false);
            _agentBehavior.SetGoal<HiddenGoal>(true);
        }

        #endregion

        private IEnumerator WaitAndReturnToPatrol()
        {
            yield return new WaitForSeconds(5f);

            if (!_isPlayerInRange)
            {
                _agentBehavior.SetGoal<KillPlayerGoal>(false);
                _agentBehavior.SetGoal<PatrolGoal>(true);
            }

            _exitCoroutine = null;
        }

        private void OnDamaged()
        {
            playerSensor.collider.radius = 40;
            _agentBehavior.SetGoal<KillPlayerGoal>(true);
        }
    }
}
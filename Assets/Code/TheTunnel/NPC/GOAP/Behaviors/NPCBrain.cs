using System;
using System.Collections;
using System.Collections.Generic;
using cowsins;
using CrashKonijn.Goap.Behaviours;
using TheTunnel.NPC;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace TheTunnel.GOAP
{
    [RequireComponent(typeof(AgentBehaviour))]
    public class NPCBrain : MonoBehaviour
    {
        private EnemySensor _enemySensor;
        private AgentBehaviour _agentBehavior;
        private List<Transform> _enemies = new List<Transform>();
        private bool isMoving;

        private void Awake()
        {
            _agentBehavior = GetComponent<AgentBehaviour>();
            _enemySensor = GetComponentInChildren<EnemySensor>();
        }

        private void Start()
        {
            _agentBehavior.SetGoal<MoveToLocationGoal>(false);
            _enemySensor.collider.radius = 20;
            _enemySensor.OnEnemyEnter += NPCSensorOnEnemyEnter;
            // _enemySensor.OnEnemyExit += NPCSensorOnEnemyExit;
        }

        private void NPCSensorOnEnemyEnter(Transform enemy)
        {
            _enemies.Add(enemy);
            _agentBehavior.SetGoal<MoveToLocationGoal>(false);
            _agentBehavior.SetGoal<AttackEnemyGoal>(true);
            enemy.GetComponent<EnemyHealth>().events.OnDeath.AddListener(() =>
            {
                _enemies.Remove(enemy);
                _agentBehavior.EndAction(true);
            });
        }

        private void NPCSensorOnEnemyExit(Vector3 lastKnownPosition)
        {
            _agentBehavior.SetGoal<AttackEnemyGoal>(false);
            _agentBehavior.SetGoal<MoveToLocationGoal>(true);
        }

        private IEnumerator WaitAndReturnGoal()
        {
            yield return new WaitForSeconds(3f);
            _agentBehavior.SetGoal<AttackEnemyGoal>(false);
            _agentBehavior.SetGoal<MoveToLocationGoal>(true);
        }

        private void Update()
        {
            if (_enemies.Count <= 0)
            {
                if (isMoving)
                {
                    isMoving = false;
                    _agentBehavior.SetGoal<AttackEnemyGoal>(false);
                    _agentBehavior.SetGoal<MoveToLocationGoal>(true);
                }
            }
            else
            {
                isMoving = true;
            }
        }
    }
}
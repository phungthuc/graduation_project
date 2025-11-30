using System;
using System.Collections;
using System.Collections.Generic;
using TheTunnel.Manager;
using TheTunnel.Target;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace TheTunnel.Enemy
{
    public class EnemyMovement : NetworkBehaviour
    {
        [SerializeField]
        private AudioClip moveAudioClip;

        [SerializeField]
        private bool isHasGapTimeBetweenSteps = true;

        public float speed = 1f;
        public NetworkVariable<bool> isMoving = new NetworkVariable<bool>(true);
        public float scanDistance;
        public LayerMask scanLayer;

        private NavMeshAgent _agent;
        private Transform _targetTransform;
        private readonly float _rotationSpeed = 2f;
        private float _stepTimmer = 0f;
        private float _gapTimeBetweenSteps = 0f;

        private void Start()
        {
            if (_agent != null)
            {
                _gapTimeBetweenSteps = isHasGapTimeBetweenSteps ? 1f - _agent.speed : moveAudioClip.length;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }

        private void OnEnable()
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_agent != null)
            {
                _agent.speed = speed;
            }

            GameObject target = GameObject.FindWithTag("Target");
            if (target != null)
            {
                _targetTransform = target.transform;
            }
            // wait to find castle
            StartCoroutine(Delay());
        }

        private IEnumerator Delay()
        {
            yield return new WaitForSeconds(1);
            Vector3 direction = transform.forward;

            // Perform the raycast
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, scanDistance, scanLayer))
            {
                _targetTransform = hit.transform;
                if (_agent != null)
                {
                    _agent.destination = hit.point;
                }
            }
        }

        private void OnDisable()
        {
            _targetTransform = null;
        }

        private void FixedUpdate()
        {
            if (!IsServer) return; // Chỉ server điều khiển movement

            if (!isMoving.Value || _targetTransform == null)
            {
                return;
            }
            FootSteps();
            HandleRotation(_targetTransform);
        }

        private void FootSteps()
        {
            if (moveAudioClip == null || !isMoving.Value || _agent == null)
            {
                if (_agent != null)
                {
                    _stepTimmer = 1f - _agent.speed;
                }
                return;
            }

            _stepTimmer -= Time.deltaTime * _agent.speed;
            if (_stepTimmer <= 0)
            {
                GameSoundManager.Instance.PlaySound(moveAudioClip, 0, true, 1f, transform.position);
                _stepTimmer = 1f - _agent.speed;
            }
        }

        public void SetMoving(bool value)
        {
            if (!IsServer) return;

            if (_agent == null || !_agent.isOnNavMesh)
            {
                return;
            }

            if (isMoving.Value == value)
            {
                return;
            }
            isMoving.Value = value;
            _agent.isStopped = !value;
            if (!value)
            {
                _agent.velocity = Vector3.zero;
            }
        }

        private void HandleRotation(Transform target)
        {
            if (target == null)
            {
                return;
            }
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * _rotationSpeed);
        }
    }
}
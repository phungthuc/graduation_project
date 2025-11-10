using cowsins;
using UnityEngine;

namespace TheTunnel.NPC
{
    public class EnemySensor : MonoBehaviour
    {
        public SphereCollider collider;
        public Collider[] _colliders = new Collider[50];
        public delegate void EnemyEnterEvent(Transform Enemy);
        public delegate void EnemyExitEvent(Vector3 lastKnownPosition);

        public event EnemyEnterEvent OnEnemyEnter;
        public event EnemyExitEvent OnEnemyExit;

        private void Awake()
        {
            collider = GetComponent<SphereCollider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                OnEnemyEnter?.Invoke(other.transform);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                OnEnemyExit?.Invoke(other.transform.position);
            }
        }
    }
}
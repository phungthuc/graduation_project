using cowsins;
using UnityEngine;

namespace TheTunnel
{
    public class EnemyProjectile : MonoBehaviour
    {
        [HideInInspector] public Vector3 dir;

        [HideInInspector] public float damage, speed;

        void Update()
        {
            transform.Translate(dir * speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerStats player = other.GetComponent<PlayerStats>();
                player.Damage(damage, false);
                Destroy(gameObject);
            }
            else if (other.CompareTag("Untagged"))
            {
                Destroy(gameObject);
            }
        }
    }
}

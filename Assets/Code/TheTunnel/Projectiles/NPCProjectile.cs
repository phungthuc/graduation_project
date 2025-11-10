using cowsins;
using UnityEngine;

namespace TheTunnel
{
    public class NPCProjectile : MonoBehaviour
    {
        [HideInInspector] public Vector3 dir;

        [HideInInspector] public float damage, speed;

        void Update()
        {
            transform.Translate(dir * speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                EnemyHealth enemy = other.GetComponent<EnemyHealth>();
                enemy.Damage(damage, false);
                Destroy(gameObject);
            }
            else if (other.CompareTag("Untagged"))
            {
                Destroy(gameObject);
            }
        }
    }
}

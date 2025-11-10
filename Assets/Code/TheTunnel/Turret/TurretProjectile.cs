using cowsins;
using TheTunnel.Core;
using UnityEngine;

namespace TheTunnel.Turret.Projectiles
{
    public class TurretProjectile : MonoBehaviour
    {
        [HideInInspector] public Vector3 dir;

        [HideInInspector] public float damage, speed;

        void Update()
        {
            transform.Translate(dir * speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(GameConstant.ENEMY_TAG))
            {
                IDamageable enemy = other.GetComponent<IDamageable>();
                if (enemy == null)
                {
                    return;
                }
                enemy.Damage(damage, false);
                Destroy(gameObject);
            }
        }
    }
}

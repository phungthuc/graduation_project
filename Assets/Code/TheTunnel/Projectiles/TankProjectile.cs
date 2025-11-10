using cowsins;
using TheTunnel.Core;
using TheTunnel.Target;
using UnityEngine;

namespace TheTunnel.Projectile
{
    public class TankProjectile : MonoBehaviour
    {
        [HideInInspector] public Vector3 dir;

        [HideInInspector] public float damage, speed;

        void Update()
        {
            transform.Translate(dir * speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(GameConstant.PLAYER_TAG))
            {
                PlayerStats player = other.GetComponent<PlayerStats>();
                player.Damage(damage, false);
                Destroy(gameObject);
            }
            
            if (other.CompareTag(GameConstant.CASTLE_TAG))
            {
                Castle.Instance.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
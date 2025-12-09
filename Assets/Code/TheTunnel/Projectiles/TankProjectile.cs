using cowsins;
using TheTunnel.Core;
using TheTunnel.Target;
using Unity.Netcode;
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
            // Chỉ server mới xử lý damage
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            if (other.CompareTag(GameConstant.PLAYER_TAG))
            {
                PlayerStats player = other.GetComponent<PlayerStats>();
                if (player != null)
                {
                    player.Damage(damage, false);
                }
                Destroy(gameObject);
            }

            if (other.CompareTag(GameConstant.CASTLE_TAG))
            {
                // Chỉ server mới gọi TakeDamage cho Castle
                if (Castle.Instance != null)
                {
                    Castle.Instance.TakeDamage(damage);
                }
                else
                {
                    Debug.LogWarning("[TankProjectile] Castle.Instance is null");
                }
                Destroy(gameObject);
            }
        }
    }
}
using System;
using cowsins;
using Unity.Netcode;
using UnityEngine;

namespace TheTunnel.GOAP
{
    [RequireComponent(typeof(Collider))]
    public class DungeonEnemyWeapom : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource;

        private bool _canHit = true;

        private void OnTriggerEnter(Collider other)
        {
            // CHỈ server mới xử lý damage (tránh duplicate damage trên các clients)
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            if (!_canHit)
            {
                return;
            }
            _canHit = false;

            if (other.CompareTag("Player"))
            {
                // Kiểm tra NetworkObject để đảm bảo đây là player object
                NetworkObject playerNetworkObject = other.GetComponent<NetworkObject>();
                if (playerNetworkObject != null && playerNetworkObject.IsSpawned)
                {
                    PlayerStats playerStats = other.GetComponent<PlayerStats>();
                    if (playerStats != null)
                    {
                        // Gọi Damage trên server (chỉ player bị tấn công mới nhận damage)
                        playerStats.Damage(1, false);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            _canHit = true;
        }
    }
}
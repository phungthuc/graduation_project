using System;
using cowsins;
using TheTunnel.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace TheTunnel.Target
{
    public class Castle : NetworkBehaviour
    {
        [SerializeField]
        private int maxHealth = 5;
        [SerializeField]
        private Slider healthSlider;

        private PlayerStats _playerStats;

        public static Castle Instance;

        // NetworkVariable để đồng bộ health cho tất cả clients
        private readonly NetworkVariable<int> _networkHealth = new(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Subscribe to network health changes
            _networkHealth.OnValueChanged += OnHealthChanged;

            // Initialize health trên server
            if (IsServer)
            {
                _networkHealth.Value = maxHealth;
            }

            // Initialize UI
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = _networkHealth.Value;
            }

            // Tìm player stats (có thể có nhiều players trong multiplayer)
            FindPlayerStats();
        }

        public override void OnNetworkDespawn()
        {
            _networkHealth.OnValueChanged -= OnHealthChanged;
            base.OnNetworkDespawn();
        }

        private void FindPlayerStats()
        {
            // Tìm tất cả players và lấy PlayerStats của player đầu tiên
            GameObject[] players = GameObject.FindGameObjectsWithTag(GameConstant.PLAYER_TAG);
            if (players != null && players.Length > 0)
            {
                foreach (var player in players)
                {
                    var playerStats = player.GetComponent<PlayerStats>();
                    if (playerStats != null)
                    {
                        _playerStats = playerStats;
                        break;
                    }
                }
            }

            if (_playerStats == null)
            {
                Debug.LogWarning("[Castle] PlayerStats not found");
                //Delay to find player stats
                StartCoroutine(DelayToFindPlayerStats());
            }
        }

        private IEnumerator DelayToFindPlayerStats()
        {
            yield return new WaitForSeconds(1f);
            FindPlayerStats();
        }

        /// <summary>
        /// Callback khi network health thay đổi (đồng bộ từ server)
        /// </summary>
        private void OnHealthChanged(int previousValue, int newValue)
        {
            UpdateHealthUI(newValue);

            // Nếu health <= 0, trigger death
            if (newValue <= 0 && previousValue > 0)
            {
                TriggerDeathClientRpc();
            }
        }

        /// <summary>
        /// Cập nhật UI health slider
        /// </summary>
        private void UpdateHealthUI(int health)
        {
            if (healthSlider != null)
            {
                healthSlider.value = health;
            }
        }

        /// <summary>
        /// Nhận damage từ enemy (chỉ server mới xử lý)
        /// </summary>
        public void TakeDamage(float damage)
        {
            Debug.Log("TakeDamage: " + damage);
            // Chỉ server mới xử lý damage
            if (!IsServer)
            {
                return;
            }

            int newHealth = _networkHealth.Value - (int)damage;
            newHealth = Mathf.Max(0, newHealth); // Đảm bảo không âm

            _networkHealth.Value = newHealth;
        }

        /// <summary>
        /// ClientRpc để trigger death trên tất cả clients
        /// </summary>
        [ClientRpc]
        private void TriggerDeathClientRpc()
        {
            // Tìm lại player stats nếu chưa có (có thể player spawn sau)
            if (_playerStats == null)
            {
                FindPlayerStats();
            }

            // Gọi Die() trên tất cả players
            if (_playerStats != null)
            {
                _playerStats.Die();
            }

            Die();
        }

        private void Die()
        {
            // Logic khi castle chết
        }
    }
}
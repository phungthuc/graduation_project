using System;
using cowsins;
using TheTunnel.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TheTunnel.Target
{
    public class Castle : MonoBehaviour
    {
        [SerializeField]
        private int maxHealth = 5;
        [SerializeField]
        private Slider healthSlider;

        private PlayerStats _playerStats;

        public static Castle Instance;
        private int _health;

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

        private void Start()
        {
            _health = maxHealth;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = _health;
            GameObject player = GameObject.FindWithTag(GameConstant.PLAYER_TAG);
            if (player == null)
            {
                Debug.Log("Player not found");
                return;
            }
            _playerStats = player.GetComponent<PlayerStats>();
        }

        public void TakeDamage(float damage)
        {
            _health -= (int)damage;
            healthSlider.value = _health;
            if (_health <= 0)
            {
                _playerStats.Die();
                Die();
            }
        }

        private void Die()
        {

        }
    }
}
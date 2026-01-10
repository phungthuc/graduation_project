using System.Collections;
using Unity.Netcode;
using UnityEngine;
using cowsins;

namespace TheTunnel.Enemy
{
    public class EnemyHealth : cowsins.EnemyHealth
    {
        private EnemyBase enemyBase;
        private NetworkObject networkObject;

        private void Awake()
        {
            enemyBase = GetComponent<EnemyBase>();
            networkObject = GetComponent<NetworkObject>();
        }

        private bool _isSubscribed = false;

        private void OnEnable()
        {
            if (enemyBase == null)
            {
                enemyBase = GetComponent<EnemyBase>();
            }
            if (networkObject == null)
            {
                networkObject = GetComponent<NetworkObject>();
            }

            StartCoroutine(SubscribeWhenReady());

            StartCoroutine(SetupHealthBarBillboardDelayed());
        }


        private System.Collections.IEnumerator SetupHealthBarBillboardDelayed()
        {
            yield return null;

            SetupHealthBarBillboard();
        }

        private void OnDisable()
        {
            UnsubscribeFromNetworkVariables();
            StopAllCoroutines();
        }

        private System.Collections.IEnumerator SubscribeWhenReady()
        {
            while (networkObject == null || !networkObject.IsSpawned || enemyBase == null)
            {
                yield return null;
            }

            if (!_isSubscribed)
            {
                SubscribeToNetworkVariables();
            }
        }

        public override void Start()
        {
            base.Start();

            if (networkObject != null && networkObject.IsSpawned && enemyBase != null && !_isSubscribed)
            {
                SubscribeToNetworkVariables();
            }

            SetupHealthBarBillboard();
        }

        private void SetupHealthBarBillboard()
        {
            if (healthSlider == null) return;

            Transform currentTransform = healthSlider.transform;
            Canvas canvas = null;

            while (currentTransform != null && canvas == null)
            {
                canvas = currentTransform.GetComponent<Canvas>();
                if (canvas == null)
                {
                    currentTransform = currentTransform.parent;
                }
            }

            Transform healthBarParent = canvas != null ? canvas.transform : healthSlider.transform.parent;
            if (healthBarParent == null)
            {
                healthBarParent = healthSlider.transform;
            }

            var existingLookAt = healthBarParent.GetComponent<cowsins.LookAt>();
            var existingBillboard = healthBarParent.GetComponent<TheTunnel.UI.HealthBarBillboard>();

            if (existingBillboard == null)
            {
                healthBarParent.gameObject.AddComponent<TheTunnel.UI.HealthBarBillboard>();

                if (existingLookAt != null)
                {
                    existingLookAt.enabled = false;
                }
            }
        }

        private void SubscribeToNetworkVariables()
        {
            if (enemyBase == null || _isSubscribed) return;

            enemyBase.networkHealth.OnValueChanged += OnHealthChanged;
            enemyBase.networkMaxHealth.OnValueChanged += OnMaxHealthChanged;
            _isSubscribed = true;

            health = enemyBase.networkHealth.Value;
            maxHealth = enemyBase.networkMaxHealth.Value;
            UpdateHealthUI();
        }

        private void UnsubscribeFromNetworkVariables()
        {
            if (enemyBase == null || !_isSubscribed) return;

            enemyBase.networkHealth.OnValueChanged -= OnHealthChanged;
            enemyBase.networkMaxHealth.OnValueChanged -= OnMaxHealthChanged;
            _isSubscribed = false;
        }


        private bool IsServer()
        {
            return enemyBase != null && enemyBase.IsServer;
        }

        private void OnHealthChanged(float previousValue, float newValue)
        {
            health = newValue;
            UpdateHealthUI();
        }

        private void OnMaxHealthChanged(float previousValue, float newValue)
        {
            maxHealth = newValue;
            UpdateHealthUI();
        }

        private void UpdateHealthUI()
        {
            if (healthSlider != null)
            {
                healthSlider.value = health;
                if (healthSlider.maxValue != maxHealth && maxHealth > 0)
                {
                    healthSlider.maxValue = maxHealth;
                }
            }
        }

        public void SetHealth(float value)
        {
            if (IsServer() && enemyBase != null)
            {
                enemyBase.networkMaxHealth.Value = value;
                maxHealth = value;
                ResetHealth();
            }
        }

        public void ResetHealth()
        {
            if (!IsServer() || enemyBase == null) return;

            isDead = false;
            enemyBase.networkHealth.Value = enemyBase.networkMaxHealth.Value;
            health = enemyBase.networkMaxHealth.Value;

            if (healthSlider != null)
            {
                healthSlider.value = health;
            }
        }

        public override void Damage(float damageAmount, bool isHeadshot = false)
        {
            if (enemyBase == null)
            {
                enemyBase = GetComponent<EnemyBase>();
            }

            if (NetworkManager.Singleton == null)
            {
                Debug.LogWarning("EnemyHealth.Damage: NetworkManager.Singleton is null");
                return;
            }

            if (enemyBase == null)
            {
                Debug.LogWarning($"EnemyHealth.Damage: enemyBase is null on GameObject {gameObject.name}. Enemy may not have EnemyBase component.");
                return;
            }

            if (!NetworkManager.Singleton.IsServer)
            {
                DamageServerRpc(damageAmount, isHeadshot);
                return;
            }

            ApplyDamage(damageAmount, isHeadshot);
        }

        [ServerRpc(RequireOwnership = false)]
        private void DamageServerRpc(float damageAmount, bool isHeadshot, ServerRpcParams rpcParams = default)
        {
            if (enemyBase == null)
            {
                enemyBase = GetComponent<EnemyBase>();
            }

            if (enemyBase == null)
            {
                Debug.LogWarning($"EnemyHealth.DamageServerRpc: enemyBase is null on GameObject {gameObject.name}");
                return;
            }

            ApplyDamage(damageAmount, isHeadshot);
        }

        private void ApplyDamage(float damageAmount, bool isHeadshot)
        {
            if (isDead) return;

            if (enemyBase == null)
            {
                Debug.LogWarning($"EnemyHealth.ApplyDamage: enemyBase is null on GameObject {gameObject.name}");
                return;
            }

            float oldHealth = health;
            float oldShield = shield;
            float damage = Mathf.Abs(damageAmount);

            if (damage <= shield)
            {
                shield -= damage;
            }
            else
            {
                damage = damage - shield;
                shield = 0;
                health -= damage;
            }

            enemyBase.networkHealth.Value = health;

            if (health <= 0)
            {
                health = 0;
                enemyBase.networkHealth.Value = 0;
                Die();
            }
            else
            {
                events.OnDamaged?.Invoke();
            }

            UpdateHealthUI();
        }

        public override void Die()
        {
            if (!IsServer()) return;

            isDead = true;

            events.OnDeath?.Invoke();

            bool shouldDestroy = DestroyOnDie;

            if (!shouldDestroy)
            {
                if (dieSFX != null)
                {
                    SoundManager.Instance.PlaySound(dieSFX, 0, 1, false, 0);
                }

                if (showKillFeed)
                {
                    UIEvents.onEnemyKilled?.Invoke(_name);
                }

                if (this != null && gameObject != null && gameObject.activeInHierarchy && enabled)
                {
                    StartCoroutine(DespawnAfterDelay(3f));
                }
                else
                {
                    if (networkObject != null && networkObject.IsSpawned)
                    {
                        networkObject.Despawn();
                    }
                }
            }
            else
            {
                base.Die();
            }
        }

        private IEnumerator DespawnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (this != null && gameObject != null && gameObject.activeInHierarchy)
            {
                if (IsServer() && networkObject != null && networkObject.IsSpawned)
                {
                    networkObject.Despawn();
                }
            }
        }
    }
}
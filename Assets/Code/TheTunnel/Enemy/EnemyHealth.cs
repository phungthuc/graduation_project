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
            // Đảm bảo enemyBase được khởi tạo khi object được enable (từ pool)
            if (enemyBase == null)
            {
                enemyBase = GetComponent<EnemyBase>();
            }
            if (networkObject == null)
            {
                networkObject = GetComponent<NetworkObject>();
            }

            // Start coroutine để check và subscribe
            StartCoroutine(SubscribeWhenReady());

            // Setup health bar billboard (chạy trên tất cả clients)
            // Sử dụng coroutine để đảm bảo healthSlider đã được khởi tạo
            StartCoroutine(SetupHealthBarBillboardDelayed());
        }

        /// <summary>
        /// Setup health bar billboard với delay để đảm bảo healthSlider đã được khởi tạo
        /// </summary>
        private System.Collections.IEnumerator SetupHealthBarBillboardDelayed()
        {
            // Đợi một frame để đảm bảo healthSlider đã được khởi tạo từ base.Start()
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
            // Đợi cho đến khi NetworkObject đã spawn và EnemyBase đã sẵn sàng
            while (networkObject == null || !networkObject.IsSpawned || enemyBase == null)
            {
                yield return null;
            }

            // Subscribe sau khi đã sẵn sàng
            if (!_isSubscribed)
            {
                SubscribeToNetworkVariables();
            }
        }

        public override void Start()
        {
            base.Start(); // Gọi base Start() để initialize health từ base class

            // Fallback: Đảm bảo subscribe nếu chưa subscribe
            if (networkObject != null && networkObject.IsSpawned && enemyBase != null && !_isSubscribed)
            {
                SubscribeToNetworkVariables();
            }

            // Đảm bảo health bar có script LookAt để quay về local player camera
            SetupHealthBarBillboard();
        }

        /// <summary>
        /// Setup health bar billboard để quay về local player camera
        /// Script này chạy trên TẤT CẢ clients (không chỉ server) để mỗi client thấy health bar quay về camera của mình
        /// </summary>
        private void SetupHealthBarBillboard()
        {
            if (healthSlider == null) return;

            // Tìm parent Canvas (thường là Canvas chứa health slider)
            // Health slider thường nằm trong: Enemy -> Canvas -> HealthSlider
            Transform currentTransform = healthSlider.transform;
            Canvas canvas = null;

            // Tìm Canvas parent (có thể là parent trực tiếp hoặc parent của parent)
            while (currentTransform != null && canvas == null)
            {
                canvas = currentTransform.GetComponent<Canvas>();
                if (canvas == null)
                {
                    currentTransform = currentTransform.parent;
                }
            }

            // Nếu không tìm thấy Canvas, sử dụng parent của healthSlider
            Transform healthBarParent = canvas != null ? canvas.transform : healthSlider.transform.parent;
            if (healthBarParent == null)
            {
                // Nếu không có parent, sử dụng chính healthSlider transform
                healthBarParent = healthSlider.transform;
            }

            // Kiểm tra xem đã có script LookAt hoặc HealthBarBillboard chưa
            var existingLookAt = healthBarParent.GetComponent<cowsins.LookAt>();
            var existingBillboard = healthBarParent.GetComponent<TheTunnel.UI.HealthBarBillboard>();

            // Nếu chưa có script nào, thêm HealthBarBillboard
            // QUAN TRỌNG: Script này chạy trên TẤT CẢ clients, không chỉ server
            // Mỗi client sẽ có HealthBarBillboard riêng để quay health bar về camera của chính họ
            if (existingBillboard == null)
            {
                // Thêm HealthBarBillboard (ưu tiên hơn LookAt vì có logic tìm camera tốt hơn)
                healthBarParent.gameObject.AddComponent<TheTunnel.UI.HealthBarBillboard>();

                // Nếu có LookAt cũ, disable nó để tránh conflict
                if (existingLookAt != null)
                {
                    existingLookAt.enabled = false;
                }
            }
        }

        private void SubscribeToNetworkVariables()
        {
            if (enemyBase == null || _isSubscribed) return;

            // Subscribe to network health changes trên tất cả clients (bao gồm cả server)
            enemyBase.networkHealth.OnValueChanged += OnHealthChanged;
            enemyBase.networkMaxHealth.OnValueChanged += OnMaxHealthChanged;
            _isSubscribed = true;

            // Initialize health từ network variable ngay lập tức
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
            // Update UI trên tất cả clients
            UpdateHealthUI();
        }

        private void OnMaxHealthChanged(float previousValue, float newValue)
        {
            maxHealth = newValue;
            UpdateHealthUI();
        }

        private void UpdateHealthUI()
        {
            // Update UI trên tất cả clients
            if (healthSlider != null)
            {
                healthSlider.value = health;
                // Đảm bảo max value cũng được set
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

        // Override Damage method để xử lý qua network
        public override void Damage(float damageAmount, bool isHeadshot = false)
        {
            // Lazy initialization: Nếu enemyBase null, thử GetComponent lại (có thể object được lấy từ pool)
            if (enemyBase == null)
            {
                enemyBase = GetComponent<EnemyBase>();
            }

            // Kiểm tra NetworkManager
            if (NetworkManager.Singleton == null)
            {
                Debug.LogWarning("EnemyHealth.Damage: NetworkManager.Singleton is null");
                return;
            }

            // Kiểm tra enemyBase sau khi lazy init
            if (enemyBase == null)
            {
                Debug.LogWarning($"EnemyHealth.Damage: enemyBase is null on GameObject {gameObject.name}. Enemy may not have EnemyBase component.");
                return;
            }

            // Nếu là client, gửi request đến server
            if (!NetworkManager.Singleton.IsServer)
            {
                DamageServerRpc(damageAmount, isHeadshot);
                return;
            }

            // Server xử lý damage trực tiếp
            ApplyDamage(damageAmount, isHeadshot);
        }

        [ServerRpc(RequireOwnership = false)]
        private void DamageServerRpc(float damageAmount, bool isHeadshot, ServerRpcParams rpcParams = default)
        {
            // Lazy initialization trong ServerRpc (có thể được gọi từ client)
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

            // Đảm bảo enemyBase không null
            if (enemyBase == null)
            {
                Debug.LogWarning($"EnemyHealth.ApplyDamage: enemyBase is null on GameObject {gameObject.name}");
                return;
            }

            float oldHealth = health;
            float oldShield = shield;
            float damage = Mathf.Abs(damageAmount);

            if (damage <= shield) // Shield will be damaged
            {
                shield -= damage;
            }
            else
            {
                damage = damage - shield;
                shield = 0;
                health -= damage;
            }

            // Sync với network
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

            // Xử lý death effects và events
            events.OnDeath?.Invoke();

            // Gọi base.Die() để xử lý sound, killfeed, death effect
            // Nhưng chỉ nếu destroyOnDie = false, vì nếu = true thì base.Die() sẽ destroy GameObject ngay
            bool shouldDestroy = DestroyOnDie; // Sử dụng property từ base class

            if (!shouldDestroy)
            {
                // Xử lý death effects manually để tránh destroy GameObject
                // Play death sound
                if (dieSFX != null)
                {
                    SoundManager.Instance.PlaySound(dieSFX, 0, 1, false, 0);
                }

                // Show killfeed nếu cần
                if (showKillFeed)
                {
                    UIEvents.onEnemyKilled?.Invoke(_name);
                }

                // Spawn death effect - sử dụng base class method hoặc reflection
                // Tạm thời bỏ qua vì deathEffect là private trong base class
                // Có thể spawn death effect trong base.Die() nếu cần

                // Start coroutine để despawn sau delay
                // Đảm bảo GameObject vẫn active trước khi start coroutine
                if (this != null && gameObject != null && gameObject.activeInHierarchy && enabled)
                {
                    StartCoroutine(DespawnAfterDelay(3f));
                }
                else
                {
                    // Nếu GameObject đã inactive, despawn ngay
                    if (networkObject != null && networkObject.IsSpawned)
                    {
                        networkObject.Despawn();
                    }
                }
            }
            else
            {
                // Nếu destroyOnDie = true, gọi base.Die() để destroy
                // Nhưng không start coroutine vì GameObject sẽ bị destroy ngay
                base.Die();
            }
        }

        private IEnumerator DespawnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Kiểm tra GameObject vẫn còn active và spawned
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
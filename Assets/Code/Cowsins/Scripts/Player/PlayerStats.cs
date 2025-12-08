/// <summary>
/// This script belongs to cowsins� as a part of the cowsins� FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
using UnityEngine.Events;
using cowsins;
using Unity.Netcode;
using System.Reflection;
namespace cowsins
{
    [System.Serializable]
    public class PlayerStats : NetworkBehaviour, IDamageable
    {
        [System.Serializable]
        public class Events
        {
            public UnityEvent OnDeath, OnDamage, OnHeal;
        }

        #region variables

        [ReadOnly]
        public float health, shield;

        public float maxHealth, maxShield, damageMultiplier, healMultiplier;

        [Tooltip("Turn on to apply damage on falling from great height")] public bool takesFallDamage;

        [Tooltip("Minimum height ( in units ) the player has to fall from in order to take damage"), SerializeField, Min(1)] private float minimumHeightDifferenceToApplyDamage;

        [Tooltip("How the damage will increase on landing if the damage on fall is going to be applied"), SerializeField] private float fallDamageMultiplier;

        [SerializeField] private bool enableAutoHeal = false;

        public bool EnableAutoHeal { get { return enableAutoHeal; } }

        [SerializeField, Min(0)] private float healRate;

        [SerializeField] private float healAmount;


        [SerializeField] private bool restartAutoHealAfterBeingDamaged = false;

        public bool RestartAutoHealAfterBeingDamaged { get { return restartAutoHealAfterBeingDamaged; } }

        [SerializeField] private float restartAutoHealTime;

        public float? height = null;

        [HideInInspector] public bool isDead;

        private PlayerMovement player;

        private PlayerStats stats;

        private PlayerStates states;

        public Events events;

        #endregion

        private void Start()
        {
            GetAllReferences();
            // Apply basic settings 
            health = maxHealth;
            shield = maxShield;
            damageMultiplier = 1;
            healMultiplier = 1;

            UIEvents.basicHealthUISetUp?.Invoke(health, shield, maxHealth, maxShield);

            GrantControl();

            if (enableAutoHeal)
                StartAutoHeal();
        }

        private void Update()
        {
            if (!IsOwner) return;
            Controllable = controllable;

            if (stats.isDead) return; // If player is dead, continue

            if (health <= 0) Die(); // Die in case we ran out of health   

            // Manage fall damage
            if (!takesFallDamage || player.Climbing) return;
            ManageFallDamage();
        }
        /// <summary>
        /// Our Player Stats is IDamageable, which means it can be damaged
        /// If so, call this method to damage the player
        /// </summary>
        public void Damage(float _damage, bool isHeadshot)
        {
            // CHỈ server mới xử lý damage (tránh duplicate damage trên các clients)
            // Điều này đảm bảo chỉ player bị tấn công mới nhận damage, không phải tất cả players
            if (!IsServer)
            {
                return;
            }

            // Early return if player is dashing with damage protection
            if (player.canDash && player.dashing && player.damageProtectionWhileDashing)
                return;

            // Ensure damage is a positive value
            float damage = Mathf.Abs(_damage);

            // Apply damage to shield first
            if (damage <= shield)
            {
                shield -= damage;
            }
            else
            {
                // Apply remaining damage to health
                damage -= shield;
                shield = 0;
                health -= damage;
            }

            // Sync damage effects và UI đến owner client (để hiển thị visual feedback)
            // Sử dụng ClientRpcParams để chỉ gửi đến owner của player này
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { OwnerClientId }
                }
            };
            DamageEffectsClientRpc(health, shield, isHeadshot, clientRpcParams);

            // Handle auto-healing
            if (enableAutoHeal && restartAutoHealAfterBeingDamaged)
            {
                CancelInvoke(nameof(AutoHeal));
                InvokeRepeating(nameof(AutoHeal), restartAutoHealTime, healRate);
            }
        }

        /// <summary>
        /// ClientRpc để sync damage effects và UI đến owner client
        /// </summary>
        [ClientRpc]
        private void DamageEffectsClientRpc(float newHealth, float newShield, bool isHeadshot, ClientRpcParams rpcParams = default)
        {
            // Update health và shield từ server
            health = newHealth;
            shield = newShield;

            // Trigger damage event để hiển thị visual feedback (màn hình nhấp nháy đỏ)
            events.OnDamage.Invoke();

            // Notify UI about the health change (để update health bar và visual effects)
            UIEvents.onHealthChanged?.Invoke(health, shield, true);
        }


        public void Heal(float healAmount)
        {
            float adjustedHealAmount = Mathf.Abs(healAmount * healMultiplier);

            // If we are at full health and shield, do not heal
            if ((maxShield > 0 && shield == maxShield) || (maxShield == 0 && health == maxHealth))
            {
                return;
            }

            // Trigger heal event
            events.OnHeal.Invoke();

            // Calculate effective healing for health
            float effectiveHealForHealth = Mathf.Min(adjustedHealAmount, maxHealth - health);
            health += effectiveHealForHealth;

            // Calculate remaining heal amount after health is full
            float remainingHeal = adjustedHealAmount - effectiveHealForHealth;

            // Apply remaining heal to shield if applicable
            if (remainingHeal > 0 && maxShield > 0)
            {
                shield = Mathf.Min(shield + remainingHeal, maxShield);
            }

            // Notify UI about the health change
            UIEvents.onHealthChanged?.Invoke(health, shield, false);
        }

        /// <summary>
        /// Perform any actions On death
        /// </summary>
        public void Die()
        {
            isDead = true;
            events.OnDeath.Invoke(); // Invoke a custom event

            // Gửi ServerRpc để thông báo server về cái chết của player này
            // Server sẽ broadcast đến tất cả clients để hiển thị màn hình Lose
            if (IsOwner)
            {
                NotifyPlayerDeathServerRpc();
            }
        }

        /// <summary>
        /// ServerRpc để thông báo server khi một player chết
        /// </summary>
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void NotifyPlayerDeathServerRpc()
        {
            // Broadcast đến tất cả clients để hiển thị màn hình Lose
            ShowLoseScreenClientRpc();
        }

        /// <summary>
        /// ClientRpc để hiển thị màn hình Lose cho tất cả players
        /// </summary>
        [ClientRpc]
        private void ShowLoseScreenClientRpc()
        {
            // Mỗi client sẽ trigger OnDeath event trên TẤT CẢ PlayerStats instances để đảm bảo
            // tất cả players đều thấy màn hình Lose (mỗi player có lose screen riêng trong prefab)
            PlayerStats[] allPlayerStats = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);

            foreach (var playerStats in allPlayerStats)
            {
                if (playerStats != null && !playerStats.isDead)
                {
                    // Set isDead = true
                    playerStats.isDead = true;

                    // Disable control để ngăn players bắn súng, di chuyển, etc.
                    playerStats.LoseControl();

                    // Trigger OnDeath event để hiển thị màn hình Lose
                    // Event này đã được setup trong prefab để activate lose screen GameObject
                    playerStats.events.OnDeath?.Invoke();
                }
            }

            // Bắt đầu countdown từ 5->0 trên tất cả clients
            // Chỉ bắt đầu countdown một lần (từ instance đầu tiên)
            if (allPlayerStats.Length > 0 && allPlayerStats[0] != null)
            {
                allPlayerStats[0].StartDeathCountdown();
            }
        }

        /// <summary>
        /// Bắt đầu countdown từ 5->0 và load scene về main scene sau khi countdown xong
        /// </summary>
        private void StartDeathCountdown()
        {
            StartCoroutine(DeathCountdownCoroutine());
        }

        private System.Collections.IEnumerator DeathCountdownCoroutine()
        {
            // Hiển thị countdown UI
            UIEvents.countDownStarted?.Invoke();

            int countdown = 5;

            while (countdown > 0)
            {
                // Hiển thị số đếm ngược
                UIEvents.countDownUpdated?.Invoke(countdown.ToString());

                yield return new WaitForSeconds(1f);
                countdown--;
            }

            // Hiển thị 0 trước khi kết thúc
            UIEvents.countDownUpdated?.Invoke("0");
            yield return new WaitForSeconds(1f);

            // Ẩn countdown UI
            UIEvents.countDownFinished?.Invoke();

            // Chỉ Owner mới gửi ServerRpc để yêu cầu Host load scene
            // Các clients khác sẽ đợi Host load scene và tự động sync
            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.Shutdown();

                // Hiện tại lúc này con trỏ chuột đang bị hide do cơ chế hide cursor trong PlayerMovement.cs
                // Giúp tôi reset để khi về main scene thì con trỏ chuột được hiển thị
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                UnityEngine.SceneManagement.SceneManager.LoadScene(
                "scene_main",
                UnityEngine.SceneManagement.LoadSceneMode.Single
                );
            }
        }
        /// <summary>
        /// Basically find everything the script needs to work
        /// </summary>
        private void GetAllReferences()
        {
            stats = GetComponent<PlayerStats>();
            states = GetComponent<PlayerStates>();
            player = GetComponent<PlayerMovement>();

            if (PauseMenu.Instance == null) return;

            PauseMenu.Instance.stats = this;
        }
        /// <summary>
        /// While airborne, if you exceed a certain time, damage on fall will be applied
        /// </summary>
        private void ManageFallDamage()
        {
            // Grab current player height
            if (!player.grounded && transform.position.y > height || !player.grounded && height == null) height = transform.position.y;

            // Check if we landed, as well if our current height is lower than the original height. If so, check if we should apply damage
            if (player.grounded && height != null && transform.position.y < height)
            {
                float currentHeight = transform.position.y;

                // Transform nullable variable into a non nullable float for later operations
                float noNullHeight = height ?? default(float);

                float heightDifference = noNullHeight - currentHeight;

                // If the height difference is enough, apply damage
                if (heightDifference > minimumHeightDifferenceToApplyDamage) Damage(heightDifference * fallDamageMultiplier, false);

                // Reset height
                height = null;
            }
        }

        private void StartAutoHeal()
        {
            InvokeRepeating(nameof(AutoHeal), healRate, healRate);
        }

        private void AutoHeal()
        {
            if (shield >= maxShield && health >= maxHealth) return;

            Heal(healAmount);
        }

        public bool controllable { get; private set; } = true;

        public static bool Controllable { get; private set; }


        public void GrantControl() => controllable = true;

        public void LoseControl() => controllable = false;

        public void ToggleControl() => controllable = !controllable;

        public void CheckIfCanGrantControl()
        {
            if (PauseMenu.isPaused || isDead) return;
            GrantControl();
        }

        public void Respawn(Vector3 respawnPosition)
        {
            isDead = false;
            states.ForceChangeState(states._States.Default());
            health = maxHealth;
            shield = maxShield;
            transform.position = respawnPosition;
            player.ResetStamina();
            player.ResetDashes();
        }
    }
}
using Unity.Netcode;
using UnityEngine;

namespace TheTunnel.Enemy
{
    public class DefenseEnemyTracker : NetworkBehaviour
    {
        [SerializeField] private EnemyManager _enemyManager;

        [SerializeField] private GameObject _portalGO;

        [SerializeField] private GameObject _portalEffectGO;

        private readonly NetworkVariable<bool> _isPortalActive = new(false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _isPortalActive.OnValueChanged += OnPortalActiveChanged;

            // Server listens to enemy clear event; clients rely on network variable
            if (IsServer && _enemyManager != null)
            {
                _enemyManager.EnemyCleaned.RemoveListener(OnEnemyDefenseLevelCleaned);
                _enemyManager.EnemyCleaned.AddListener(OnEnemyDefenseLevelCleaned);
            }

            SetPortalVisibility(_isPortalActive.Value);
        }

        public override void OnNetworkDespawn()
        {
            _isPortalActive.OnValueChanged -= OnPortalActiveChanged;

            if (IsServer && _enemyManager != null)
            {
                _enemyManager.EnemyCleaned.RemoveListener(OnEnemyDefenseLevelCleaned);
            }

            base.OnNetworkDespawn();
        }

        private void OnEnemyDefenseLevelCleaned()
        {
            if (!IsServer) return;
            ActivatePortal();
        }

        private void ActivatePortal()
        {
            PlayerData.Instance.SetDefenseLevelCompleted(PlayerData.Instance.CurrentLevel);
            if (_isPortalActive.Value) return;
            _isPortalActive.Value = true;
            SetPortalVisibility(true);
        }

        private void OnPortalActiveChanged(bool previousValue, bool newValue)
        {
            SetPortalVisibility(newValue);
        }

        private void SetPortalVisibility(bool isActive)
        {
            if (_portalGO != null)
            {
                _portalGO.SetActive(isActive);
            }

            if (_portalEffectGO != null)
            {
                _portalEffectGO.SetActive(isActive);
            }
        }
    }
}

using Unity.Netcode;
using UnityEngine;
using TheTunnel;

namespace TheTunnel.Enemy.DungeonEnemy
{
    public class DungeonEnemyTracker : NetworkBehaviour
    {
        [SerializeField] private DungeonEnemyManager _enemyManager;

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
                _enemyManager.EnemyCleaned.RemoveListener(OnEnemyDungeonLevelCleaned);
                _enemyManager.EnemyCleaned.AddListener(OnEnemyDungeonLevelCleaned);
            }

            SetPortalVisibility(_isPortalActive.Value);
        }

        public override void OnNetworkDespawn()
        {
            _isPortalActive.OnValueChanged -= OnPortalActiveChanged;

            if (IsServer && _enemyManager != null)
            {
                _enemyManager.EnemyCleaned.RemoveListener(OnEnemyDungeonLevelCleaned);
            }

            base.OnNetworkDespawn();
        }

        private void OnEnemyDungeonLevelCleaned()
        {
            if (!IsServer) return;
            ActivatePortal();
        }

        private void ActivatePortal()
        {
            PlayerData.Instance.SetDungeonLevelCompleted(PlayerData.Instance.CurrentLevel);
            PlayerData.Instance.CurrentLevel++;
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

using UnityEngine;

namespace TheTunnel.Enemy
{
    public class DefenseEnemyTracker : MonoBehaviour
    {
        [SerializeField] private EnemyManager _enemyManager;

        [SerializeField] private GameObject _portalGO;

        private void Start()
        {
            if (PlayerData.Instance.IsDefenseLevelCompleted(PlayerData.Instance.CurrentLevel))
            {
                return;
            }
            _portalGO.SetActive(false);
            _enemyManager.EnemyCleaned.RemoveListener(OnEnemyDefenseLevelCleaned);
            _enemyManager.EnemyCleaned.AddListener(OnEnemyDefenseLevelCleaned);
        }

        private void OnEnemyDefenseLevelCleaned()
        {
            PlayerData.Instance.SetDefenseLevelCompleted(PlayerData.Instance.CurrentLevel);
            _portalGO.SetActive(true);
        }
    }
}

using System.Collections.Generic;
using TheTunnel.Level;
using UnityEngine;

namespace TheTunnel
{
    public class DungeonEventManager : MonoBehaviour
    {
        [SerializeField] private List<DungeonEvent> dungeonEvents;
        [SerializeField] private DungeonEnemyManager enemyManager;

        private void Awake()
        {
            if (enemyManager == null)
            {
                enemyManager = FindAnyObjectByType<DungeonEnemyManager>();
            }
        }

        private void Start()
        {
            foreach (var dungeonEvent in dungeonEvents)
            {
                dungeonEvent.SetupConfigCondition();
            }
        }

        private void Update()
        {
            foreach (var dungeonEvent in dungeonEvents)
            {
                if (dungeonEvent.IsConditionMet())
                {
                    TriggerEvent(dungeonEvent);
                }
            }
        }

        private void TriggerEvent(DungeonEvent dungeonEvent)
        {
            if (enemyManager == null || dungeonEvent.isPlayEvent) return;

            foreach (var response in dungeonEvent.ResponseList)
            {
                switch (response)
                {
                    case DungeonEventResponse.SpawnEnemies:
                        enemyManager.SpawnDungeonWave(dungeonEvent.ZoneID);
                        dungeonEvent.isPlayEvent = true;
                        break;
                    case DungeonEventResponse.SpawnBoss:
                        dungeonEvent.isPlayEvent = true;
                        break;
                    case DungeonEventResponse.SpawnItem:
                        dungeonEvent.isPlayEvent = true;
                        break;
                    case DungeonEventResponse.UnlockDoor:
                        dungeonEvent.isPlayEvent = true;
                        break;
                    case DungeonEventResponse.PlayCutscene:
                        dungeonEvent.isPlayEvent = true;
                        break;
                }
            }
        }
    }
}

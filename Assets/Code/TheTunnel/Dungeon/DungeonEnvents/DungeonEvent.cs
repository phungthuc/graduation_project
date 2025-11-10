using System.Collections.Generic;
using cowsins;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace TheTunnel
{
    public enum DungeonConditionType
    {
        PlayerInZone,
        KilledEnemiesInZone,
        KilledEnemies,
        CollectedItems,
        BossDefeated
    }
    public enum DungeonEventResponse
    {
        SpawnEnemies,
        SpawnBoss,
        UnlockDoor,
        SpawnItem,
        PlayCutscene,
    }
    public class DungeonEvent : MonoBehaviour
    {
        [SerializeField] private DungeonConditionType conditionType;
        [SerializeField] private List<DungeonEventResponse> responseList = new List<DungeonEventResponse>();
        [SerializeField] private string zoneName;
        private bool isTriggered;
        public bool isPlayEvent = false;

        #region PlayerInZone
        [SerializeField] private float zoneRadius;
        #endregion

        #region KilledEnemiesInZone
        [SerializeField] private GameObject enemiesParent;
        private List<EnemyHealth> _enemies = new List<EnemyHealth>();
        private int _aliveCount;
        public UnityEvent EnemyCleaned;
        #endregion

        #region KilledEnemies
        [SerializeField] private int requiredEnemyKills;
        #endregion

        #region KilledBoss
        [SerializeField] private string bossName;
        #endregion

        #region SpawnEnemies
        [SerializeField] private string zoneID;
        #endregion

        #region SpawnBoss
        [SerializeField] private string bossID;
        #endregion

        #region UnlockDoor

        #endregion

        #region SpawnItem

        #endregion

        #region PlayCutscene

        #endregion

        #region Getters
        public DungeonConditionType ConditionType => conditionType;
        public List<DungeonEventResponse> ResponseList => responseList;
        public string ZoneName => zoneName;
        public float ZoneRadius => zoneRadius;
        public GameObject EnemiesParent => enemiesParent;
        public int RequiredEnemyKills => requiredEnemyKills;
        public string BossName => bossName;
        public string ZoneID => zoneID;
        public string BossID => bossID;
        #endregion
        public bool IsConditionMet()
        {
            switch (conditionType)
            {
                case DungeonConditionType.PlayerInZone:
                    return CheckPlayerInZone();

                case DungeonConditionType.KilledEnemiesInZone:
                    return CheckKilledEnemies();

                case DungeonConditionType.KilledEnemies:
                    return CheckKilledEnemies();

                case DungeonConditionType.CollectedItems:
                    return CheckCollectedItems();

                default:
                    return false;
            }
        }

        public void SetupConfigCondition()
        {
            switch (conditionType)
            {
                case DungeonConditionType.PlayerInZone:
                    ConfigPlayerInZone();
                    break;
                case DungeonConditionType.KilledEnemiesInZone:
                    ConfigKilledEnemiesInZone();
                    break;
                case DungeonConditionType.KilledEnemies:
                    break;
                case DungeonConditionType.CollectedItems:
                    break;
                default:
                    break;
            }
        }

        #region PlayerInZone
        private void ConfigPlayerInZone()
        {
            gameObject.AddComponent<CapsuleCollider>();
            CapsuleCollider capsuleCollider = gameObject.GetComponent<CapsuleCollider>();
            capsuleCollider.isTrigger = true;
            capsuleCollider.radius = zoneRadius;
        }

        private bool CheckPlayerInZone()
        {
            return isTriggered;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isTriggered = true;
            }
        }
        #endregion

        #region KilledEnemiesInZone
        private void ConfigKilledEnemiesInZone()
        {
            _enemies = new List<EnemyHealth>(enemiesParent.GetComponentsInChildren<EnemyHealth>());
            _aliveCount = _enemies.Count;
            // Subscribe to health changes
            foreach (var enemy in _enemies)
            {
                enemy.events.OnDeath.AddListener(OnEnemyDeath);
            }

            EnemyCleaned.AddListener(OnEnemyLevelCleaned);
        }

        private void OnEnemyDeath()
        {
            UpdateAliveCount();
        }

        private void UpdateAliveCount()
        {
            _enemies.RemoveAll(e => e == null || e.health <= 0);
            _aliveCount = _enemies.Count;

            if (_aliveCount <= 0)
            {
                EnemyCleaned?.Invoke();
            }
        }

        private void OnEnemyLevelCleaned()
        {
            isTriggered = true;
        }
        #endregion

        private bool CheckKilledEnemies()
        {
            return isTriggered;
        }

        private bool CheckCollectedItems()
        {
            return isTriggered;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DungeonEvent))]
    public class DungeonEventEditor : Editor
    {
        SerializedProperty conditionType;
        SerializedProperty responseList;
        SerializedProperty zoneName;
        SerializedProperty zoneRadius;
        SerializedProperty enemiesParent;
        SerializedProperty requiredEnemyKills;
        SerializedProperty bossName;
        SerializedProperty zoneID;
        SerializedProperty bossID;

        private void OnEnable()
        {
            // Serialize fields
            conditionType = serializedObject.FindProperty("conditionType");
            responseList = serializedObject.FindProperty("responseList");
            zoneName = serializedObject.FindProperty("zoneName");
            zoneRadius = serializedObject.FindProperty("zoneRadius");
            enemiesParent = serializedObject.FindProperty("enemiesParent");
            requiredEnemyKills = serializedObject.FindProperty("requiredEnemyKills");
            bossName = serializedObject.FindProperty("bossName");
            zoneID = serializedObject.FindProperty("zoneID");
            bossID = serializedObject.FindProperty("bossID");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Dungeon Event Configuration", EditorStyles.boldLabel);

            // Event Condition Type
            EditorGUILayout.PropertyField(conditionType);

            DungeonConditionType condition = (DungeonConditionType)conditionType.enumValueIndex;

            // Display condition-specific fields
            switch (condition)
            {
                case DungeonConditionType.PlayerInZone:
                    EditorGUILayout.PropertyField(zoneName, new GUIContent("Zone Name"));
                    EditorGUILayout.PropertyField(zoneRadius, new GUIContent("Zone Radius"));
                    break;

                case DungeonConditionType.KilledEnemiesInZone:
                    EditorGUILayout.PropertyField(enemiesParent, new GUIContent("Enemies Parent"));
                    break;

                case DungeonConditionType.KilledEnemies:
                    EditorGUILayout.PropertyField(requiredEnemyKills, new GUIContent("Required Enemy Kills"));
                    break;

                case DungeonConditionType.BossDefeated:
                    EditorGUILayout.PropertyField(bossName, new GUIContent("Boss Name"));
                    break;
            }

            // Event Responses
            EditorGUILayout.LabelField("Event Responses", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(responseList, new GUIContent("Responses"), true);

            // Display response-specific fields
            foreach (DungeonEventResponse response in GetSelectedResponses())
            {
                switch (response)
                {
                    case DungeonEventResponse.SpawnEnemies:
                        EditorGUILayout.PropertyField(zoneID, new GUIContent("Zone ID"));
                        break;

                    case DungeonEventResponse.SpawnBoss:
                        EditorGUILayout.PropertyField(bossID, new GUIContent("Boss ID"));
                        break;

                    case DungeonEventResponse.UnlockDoor:
                        EditorGUILayout.HelpBox("This response will unlock a specific door. Ensure the door is configured in the level.", MessageType.Info);
                        break;

                    case DungeonEventResponse.SpawnItem:
                        EditorGUILayout.HelpBox("This response will spawn an item at a predefined location.", MessageType.Info);
                        break;

                    case DungeonEventResponse.PlayCutscene:
                        EditorGUILayout.HelpBox("This response will trigger a cutscene. Ensure the cutscene is properly configured.", MessageType.Info);
                        break;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private List<DungeonEventResponse> GetSelectedResponses()
        {
            List<DungeonEventResponse> selectedResponses = new List<DungeonEventResponse>();

            for (int i = 0; i < responseList.arraySize; i++)
            {
                SerializedProperty element = responseList.GetArrayElementAtIndex(i);
                selectedResponses.Add((DungeonEventResponse)element.enumValueIndex);
            }

            return selectedResponses;
        }
    }
#endif
}
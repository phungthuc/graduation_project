using System;
using UnityEditor;
using UnityEngine;

namespace TheTunnel
{
    [CreateAssetMenu(fileName = "NewDungeonEventConfig", menuName = "The Tunnel/Dungeon Event Config")]
    public class DungeonEventConfig : ScriptableObject
    {
        public DungeonConditionType conditionType;
        public DungeonEventResponse response;
        public bool repeatable = false;

        // PlayerInZone specific fields
        public string zoneName;
        public float zoneRadius;

        // KilledEnemies specific fields
        public int requiredEnemyKills;

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DungeonEventConfig))]
    public class DungeonEventConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DungeonEventConfig dungeonEvent = (DungeonEventConfig)target;

            // Hiển thị Condition Type
            dungeonEvent.conditionType = (DungeonConditionType)EditorGUILayout.EnumPopup("Condition Type", dungeonEvent.conditionType);

            // Hiển thị Event Response
            dungeonEvent.response = (DungeonEventResponse)EditorGUILayout.EnumPopup("Event Response", dungeonEvent.response);

            // Hiển thị Repeatable
            dungeonEvent.repeatable = EditorGUILayout.Toggle("Repeatable", dungeonEvent.repeatable);

            // Hiển thị các trường tùy thuộc vào Condition Type
            switch (dungeonEvent.conditionType)
            {
                case DungeonConditionType.PlayerInZone:
                    dungeonEvent.zoneName = EditorGUILayout.TextField("Zone Name", dungeonEvent.zoneName);
                    dungeonEvent.zoneRadius = EditorGUILayout.FloatField("Zone Radius", dungeonEvent.zoneRadius);
                    break;

                case DungeonConditionType.KilledEnemies:
                    dungeonEvent.requiredEnemyKills = EditorGUILayout.IntField("Required Enemy Kills", dungeonEvent.requiredEnemyKills);
                    break;

                // Thêm điều kiện khác nếu cần
                case DungeonConditionType.CollectedItems:
                case DungeonConditionType.BossDefeated:
                    EditorGUILayout.LabelField("No specific fields for this condition.");
                    break;
            }

            // Lưu thay đổi nếu có chỉnh sửa
            if (GUI.changed)
            {
                EditorUtility.SetDirty(dungeonEvent);
            }
        }
    }
#endif
}

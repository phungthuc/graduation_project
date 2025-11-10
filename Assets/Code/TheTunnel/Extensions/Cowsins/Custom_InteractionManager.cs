#if UNITY_EDITOR
using UnityEditor;
#endif
using cowsins;
using UnityEngine;
using TheTunnel.Turret;

namespace TheTunnel.Custom.cowsins
{
    public class Custom_InteractionManager : InteractManager
    {
        protected override void DetectPickeable()
        { 
            // It will be used later on to determine the hit object.
            RaycastHit interactableHit;
            LayerMask combinedMask = mask | playerMovement.whatIsGround;
    
            Debug.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * detectInteractionDistance);
            // If we got a hit from the raycast:
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out interactableHit, detectInteractionDistance, combinedMask))
            {
                // Check if the interaction is forbidden
                if (IsForbiddenInteraction(interactableHit))
                {
                    // If it is forbidden, call the event
                    UIEvents.forbiddenInteraction?.Invoke();
                }
                else
                {
                    // If its not, enable interaction UI to display an interaction
                    EnableInteractionUI(interactableHit.collider.GetComponent<Interactable>());
                }
            }
            else
            {
                // If we dont find any interactable, disable interactions UI
                DisableLookingAtInteractable();
                DisableInteractionUI();
            }
        }

        protected override bool IsForbiddenInteraction(RaycastHit hit)
        {
            TurretController turret = hit.collider.GetComponent<TurretController>();
            if (turret != null && turret.IsMagazineFull())
            {
                return true;
            }

            return base.IsForbiddenInteraction(hit);
        }

        protected override void PerformInteraction()
        {
            if (highlightedInteractable is TurretController turret)
            {
                turret.Reload();
                alreadyInteracted = true;
                Invoke(nameof(ResetInteractTimer), interactInterval);
                highlightedInteractable = null;
                UIEvents.disableInteractionUI?.Invoke();
                UIEvents.onFinishInteractionProgress?.Invoke();
                events.OnFinishedInteraction.Invoke();
                return;
            }

            base.PerformInteraction();
        }
    }

    #if UNITY_EDITOR
    [System.Serializable]
    [CustomEditor(typeof(Custom_InteractionManager))]
    public class Custom_InteractionManagerEditor : Editor
    {
        private string[] tabs = { "References", "Interaction", "Events" };
        private int currentTab = 0;

        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            Custom_InteractionManager myScript = target as Custom_InteractionManager;

            Texture2D myTexture = Resources.Load<Texture2D>("CustomEditor/interactManager_CustomEditor") as Texture2D;
            GUILayout.Label(myTexture);

            EditorGUILayout.BeginVertical();
            currentTab = GUILayout.Toolbar(currentTab, tabs);
            EditorGUILayout.Space(10f);
            EditorGUILayout.EndVertical();
            #region variables

            if (currentTab >= 0 || currentTab < tabs.Length)
            {
                switch (tabs[currentTab])
                {
                    case "References":
                        EditorGUILayout.LabelField("REFERENCES", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("mask"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponGenericPickeable"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("attachmentGenericPickeable"));
                        break;
                    case "Interaction":
                        EditorGUILayout.LabelField("INTERACTION", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField("DROP WEAPONS", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("detectInteractionDistance"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("progressRequiredToInteract"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("interactInterval"));
                        if (myScript.DuplicateWeaponAddsBullets)
                        {
                            EditorGUILayout.Space(10);
                            EditorGUILayout.LabelField("This feature is only applicable to weapons with limited magazines.", EditorStyles.helpBox);
                            EditorGUILayout.Space(5);
                        }
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("duplicateWeaponAddsBullets"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("canDrop"));
                        if (myScript.canDrop)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("droppingDistance"));
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.LabelField("INSPECTION & REALTIME CUSTOMIZATION", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("canInspect"));
                        if (myScript.canInspect)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("realtimeAttachmentCustomization"));
                            if (myScript.realtimeAttachmentCustomization)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("displayCurrentAttachmentsOnly"));
                                EditorGUI.indentLevel--;
                            }
                            EditorGUI.indentLevel--;
                        }
                        break;
                    case "Events":
                        EditorGUILayout.LabelField("OTHERS", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("events"));
                        break;

                }
            }

            #endregion
            EditorGUILayout.Space(10f);
            serializedObject.ApplyModifiedProperties();

        }
    }
    #endif
}

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace cowsins.Extensions
{
    public class PlayerConstruction : MonoBehaviour
    {
        [SerializeField] private float detectionDistance; 
        [SerializeField] private LayerMask turretSlotLayer;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private GameObject highlightGO;
        [SerializeField] private GameObject turretPrefab;
        
        [HideInInspector]
        public bool isBuilding;
        public GameObject FakeGameObject => highlightGO;
         
        private WeaponController _weaponController;
        private PlayerMovement _playerMovement;
        
        public TurretSlot CurrentSlotDetected { get; private set; }

        private void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            if (_playerMovement == null)
            {
                Debug.LogError("PlayerMovement is not found");
                return;
            }
            
            _weaponController = GetComponent<WeaponController>();
            if (_weaponController == null)
            {
                Debug.LogError("WeaponController is not found");
            }
        }

        private void Update()
        {
            if (!isBuilding)
            {
                return;
            }
            DetectBuilding();
        }
        
        public void StartBuild()
        {
            isBuilding = true;
            _weaponController.HideCurrentWeapon();
            UIEvents.onConstructStarted?.Invoke();
        }
        
        public void StopBuild()
        {
            isBuilding = false;
            _weaponController.ShowCurrentWeapon();
            highlightGO.SetActive(false);
        }

        public void Build()
        {
            if (CurrentSlotDetected == null)
            {
                return;
            }
            Instantiate(turretPrefab, CurrentSlotDetected.transform, false);
            CurrentSlotDetected.IsFilled = true;
            CurrentSlotDetected = null;
        }
        
        private void DetectBuilding()
        {
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, detectionDistance, turretSlotLayer))
            {
                GameObject hitObject = hit.collider.gameObject;
                CurrentSlotDetected = hitObject.GetComponent<TurretSlot>();
                if (!CurrentSlotDetected.IsFilled)
                {
                    highlightGO.transform.rotation = hitObject.transform.rotation;
                    highlightGO.transform.position = hitObject.transform.position;
                    highlightGO.SetActive(true);
                    return;
                }
            }
            CurrentSlotDetected = null;
            highlightGO.SetActive(false);
        }
    }
}

using System;
using System.Collections.Generic;
using cowsins;
using cowsins.Extensions;
using TheTunnel.Pool;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace TheTunnel.Custom.cowsins
{
    public class TurretInventory : MonoBehaviour
    {
        [SerializeField] private PlayerConstruction playerConstruction;
        [SerializeField] private TurretUISlot turretUISlotPrefab;
        [SerializeField] private Transform holderTransform;
        [SerializeField] private GameObject playerUI;
        [SerializeField] private GameObject container;
        
        private GameObjectPool<TurretUISlot> _turretSlotPool;
        private List<TurretUISlot> _turretSlotList = new();

        public UnityEvent<string> TurretSelected;
        
        private void Start()
        {
            _turretSlotPool = new GameObjectPool<TurretUISlot>(turretUISlotPrefab, holderTransform, 5, (TurretUISlot slot) =>
            {
                slot.gameObject.SetActive(false);
            });
            UIEvents.onConstructStarted += Open;
            PauseMenu.Instance.OnPause += Close;
        }
        
        public void Open()
        {
            SpawnUISlot();
            container.SetActive(true);
            playerUI.SetActive(false);
            PauseMenu.Instance.PauseControl();
        }
        
        public void Close()
        {
            playerConstruction.StopBuild();
            DisableSelection();
        }
        
        private void OnTurretSelected(TurretUISlot slot)
        {
           DisableSelection();
           TurretSelected?.Invoke(slot.name);
        }
        
        private void DisableSelection()
        {
            // make sure weapon not shooting on close
            // Check to remove this
            InputManager.shooting = false;
            
            PauseMenu.Instance.UnpauseControl();
            container.SetActive(false);
            playerUI.SetActive(true);
            DisableAllSlot();
        }
        
        private void SpawnUISlot()
        {
            for (int i = 0; i < 20; i++)
            {
                var slot = _turretSlotPool.GetObject();
                _turretSlotList.Add(slot);
                slot.gameObject.SetActive(true);
                slot.Selected.RemoveListener(OnTurretSelected);
                slot.Selected.AddListener(OnTurretSelected);
            }
            playerUI.SetActive(false);
        }
        
        private void DisableAllSlot()
        {
            foreach (var slot in _turretSlotList)
            {
                _turretSlotPool.ReturnObject(slot);
            }
            _turretSlotList.Clear();
        }
    }
}

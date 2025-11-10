using cowsins;
using cowsins.Extensions;
using UnityEngine;

namespace TheTunnel.Custom.cowsins
{
    public class Custom_PlayerDefaultState : PlayerDefaultState
    {
        private Custom_PlayerStateFactory _customFactory;
        private PlayerConstruction _playerConstruction;
        private WeaponController _weaponController;
      
        public Custom_PlayerDefaultState(PlayerStates currentContext, PlayerStateFactory playerStateFactory) : base(
            currentContext, playerStateFactory)
        {
            _customFactory = playerStateFactory as Custom_PlayerStateFactory;
            _playerConstruction = _ctx.GetComponent<PlayerConstruction>();
            _weaponController = _ctx.GetComponent<WeaponController>();
            if (_weaponController == null)
            {
                Debug.LogError("WeaponController component not found on player");
                return;
            }
        }

        public override void UpdateState()
        {
            if (!stats.controllable) return;
            base.UpdateState();
            if (_weaponController.Reloading) return;
            if (Custom_InputManager.startBuild && !_playerConstruction.isBuilding)
            {
                _playerConstruction.StartBuild();
            }
            
            if (Custom_InputManager.exitBuilding && _playerConstruction.isBuilding)
            {
                _playerConstruction.StopBuild();
            }
        }
    }
}

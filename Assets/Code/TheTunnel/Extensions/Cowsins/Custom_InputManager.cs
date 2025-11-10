using cowsins;
using UnityEngine;

namespace TheTunnel.Custom.cowsins
{
    public class Custom_InputManager : InputManager
    {
        public static bool startBuild;
        public static bool exitBuilding;
        public static bool exitDungeonLevel;


        public override void Update()
        {
            base.Update();
            if (PauseMenu.isPaused)
            {
                return;
            }
            startBuild = inputActions.GameControls.Build.IsPressed();
            exitBuilding = inputActions.GameControls.ExitBuild.IsPressed();
            exitDungeonLevel = inputActions.GameControls.ExitDungeonLevel.IsPressed();
        }
    }
}
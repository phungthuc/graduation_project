using cowsins;
using TheTunnel.Core;
using TheTunnel.Level;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheTunnel.Components
{
    public class DefenseLevelPortal : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == GameConstant.PLAYER_TAG)
            {
                UIController.instance.crosshair.SetVisibility(false);
                TransitionScene.Instance.PlayTransitionScene(GameConstant.DUNGEON_SCENE_NAME, () => LevelManager.Instance.LoadDungeonLevel(PlayerData.Instance.CurrentLevel));
                this.enabled = false;
            }
        }
    }
}

using System.Collections;
using TheTunnel.Components;
using TheTunnel.Core;
using TheTunnel.Level;
using TheTunnel.Manager;
using UnityEngine;

namespace TheTunnel.ScriptDebug
{
    public class LoadLevelByActiveScene : MonoBehaviour
    {

        public void DelayToLoadLevel()
        {
            StartCoroutine(DelayLoadLevel());
        }


        public void LoadLevel()
        {
            string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (activeSceneName == GameConstant.SCENE_DUNGEON_NAME)
            {
                LevelManager.Instance.LoadDungeonLevel(PlayerData.Instance.CurrentLevel);
            }
            else
            {
                CheckDefenseLevel();
            }
        }

        public void CheckDefenseLevel()
        {
            if (PlayerData.Instance.IsDefenseLevelCompleted(PlayerData.Instance.CurrentLevel))
            {
                GameObject portal = GameObject.FindFirstObjectByType<DefenseLevelPortal>().gameObject;
                portal.SetActive(true);
            }
            else
            {
                GameManager.Instance.StartCountDown();
            }
        }

        private IEnumerator DelayLoadLevel()
        {
            yield return new WaitForSeconds(1f);
            LoadLevel();
        }
    }
}

using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using TheTunnel.Core;

namespace TheTunnel.Bootstrap
{
    public class UGSInitializer : MonoBehaviour
    {
        private string _nextScene = GameConstant.SCENE_MAIN_NAME;

        async void Awake()
        {
            DontDestroyOnLoad(gameObject);
            await InitAsync();
            TransitionScene.Instance.PlayTransitionScene(_nextScene);
        }

        static async Task InitAsync()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log($"Signed-in: {AuthenticationService.Instance.PlayerId}");
        }
    }

}
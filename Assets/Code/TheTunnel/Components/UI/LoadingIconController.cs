using UnityEngine;

namespace TheTunnel.Components.UI
{
    public class LoadingIconController : MonoBehaviour
    {

        private void Update()
        {
            RotateLoadingIcon();
        }

        private void RotateLoadingIcon()
        {
            transform.Rotate(0f, 0f, Time.deltaTime * -500);
        }

    }
}
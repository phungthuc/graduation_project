using UnityEngine;
using UnityEngine.Events;

namespace TheTunnel.Custom.cowsins
{
    public class TurretUISlot : MonoBehaviour
    {
        public UnityEvent<TurretUISlot> Selected;

        public void OnSelected()
        {
            Selected?.Invoke(this);
        }
    }
}
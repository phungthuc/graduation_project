using cowsins;
using UnityEngine;

namespace TheTunnel
{
    public class HidingWall : Destructible
    {
       [SerializeField] private GameObject destroyedObject;
        public override void Die()
        {
            Instantiate(destroyedObject, transform.position, Quaternion.identity);
            if (destroyedSFX != null) SoundManager.Instance.PlaySound(destroyedSFX, 0, .1f, true, 0);
            base.Die();
        }
    }
}

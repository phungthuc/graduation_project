using System;
using TheTunnel.Manager;
using UnityEngine;

namespace TheTunnel.Weapon
{
    [RequireComponent(typeof(Collider))]
    public class PunchWeapon : MonoBehaviour
    {
        [SerializeField]
        private AudioClip hitSound;

        public event Action<GameObject> Hit;

        private bool _canHit = true;

        private void OnTriggerEnter(Collider other)
        {
            if (!_canHit)
            {
                return;
            }
            _canHit = false;

            GameSoundManager.Instance.PlaySound(hitSound, 0, true, 1f, transform.position);
            Hit?.Invoke(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            _canHit = true;
        }
    }
}
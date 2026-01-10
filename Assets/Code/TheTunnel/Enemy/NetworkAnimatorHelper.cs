using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace TheTunnel.Enemy
{
    public class NetworkAnimatorHelper : MonoBehaviour
    {
        private NetworkAnimator networkAnimator;
        private Animator animator;

        private void Awake()
        {
            networkAnimator = GetComponent<NetworkAnimator>();
            if (networkAnimator != null)
            {
                animator = networkAnimator.Animator;
            }
        }

        public void SetTrigger(int hash)
        {
            if (networkAnimator != null)
            {
                networkAnimator.SetTrigger(hash);
            }
            else if (animator != null)
            {
                animator.SetTrigger(hash);
            }
        }

        public void SetTrigger(string name)
        {
            SetTrigger(Animator.StringToHash(name));
        }

        public void SetBool(int hash, bool value)
        {
            if (animator != null)
            {
                animator.SetBool(hash, value);
            }
        }

        public void SetBool(string name, bool value)
        {
            SetBool(Animator.StringToHash(name), value);
        }

        public Animator GetAnimator()
        {
            return animator;
        }

        public NetworkAnimator GetNetworkAnimator()
        {
            return networkAnimator;
        }
    }
}


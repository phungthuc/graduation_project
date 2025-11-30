using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace TheTunnel.Enemy
{
    /// <summary>
    /// Helper class để truy cập NetworkAnimator
    /// NetworkAnimator tự động sync tất cả Animator parameters qua network
    /// </summary>
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

        /// <summary>
        /// Set trigger parameter - NetworkAnimator sẽ tự động sync
        /// </summary>
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

        /// <summary>
        /// Set trigger parameter by name
        /// </summary>
        public void SetTrigger(string name)
        {
            SetTrigger(Animator.StringToHash(name));
        }

        /// <summary>
        /// Set bool parameter - NetworkAnimator tự động sync bool parameters
        /// </summary>
        public void SetBool(int hash, bool value)
        {
            if (animator != null)
            {
                // NetworkAnimator tự động sync tất cả Animator parameters
                // Chỉ cần set trên Animator, NetworkAnimator sẽ tự sync
                animator.SetBool(hash, value);
            }
        }

        /// <summary>
        /// Set bool parameter by name
        /// </summary>
        public void SetBool(string name, bool value)
        {
            SetBool(Animator.StringToHash(name), value);
        }

        /// <summary>
        /// Get Animator để truy cập các method khác
        /// </summary>
        public Animator GetAnimator()
        {
            return animator;
        }

        /// <summary>
        /// Get NetworkAnimator
        /// </summary>
        public NetworkAnimator GetNetworkAnimator()
        {
            return networkAnimator;
        }
    }
}


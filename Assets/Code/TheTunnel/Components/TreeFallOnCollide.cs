using TheTunnel.Core;
using UnityEngine;

namespace TheTunnel.Components
{
    [RequireComponent(typeof(Collider))]
    public class TreeFallOnCollide : MonoBehaviour
    {
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(GameConstant.TREE_TAG))
            {
                BoxCollider boxCollider = other.gameObject.GetComponent<BoxCollider>();
                if (boxCollider != null)
                {
                    boxCollider.enabled = false;
                }
                var anim = other.gameObject.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.SetTrigger("Hit");
                }
            }
        }
    }
}

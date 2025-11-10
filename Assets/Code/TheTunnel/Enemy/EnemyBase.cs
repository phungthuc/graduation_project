using UnityEngine;
using UnityEngine.Events;

namespace TheTunnel.Enemy
{
    [RequireComponent(typeof(Collider))]
    public abstract class EnemyBase : MonoBehaviour
    {
        public UnityEvent Died;

        [HideInInspector] public EnemyStat stat;
        [HideInInspector] public bool isPaused;

        public virtual void Init(EnemyStat st)
        {
            stat = st;
        }

        public virtual void SetPaused(bool paused)
        {
            isPaused = paused;
        }

        public virtual void OnDied()
        {
            Died.Invoke();
        }

        public virtual void OnReset() { }
    }
}

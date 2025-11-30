using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

namespace TheTunnel.Enemy
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Collider))]
    public abstract class EnemyBase : NetworkBehaviour
    {
        public UnityEvent Died;

        [HideInInspector] public EnemyStat stat;
        [HideInInspector] public NetworkVariable<bool> isPaused = new NetworkVariable<bool>(false);
        
        // NetworkVariables để đồng bộ health (sử dụng bởi EnemyHealth)
        public NetworkVariable<float> networkHealth = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<float> networkMaxHealth = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // Chỉ server mới có quyền điều khiển Enemy
            if (!IsServer)
            {
                // Client chỉ cần theo dõi trạng thái
                return;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        }

        public virtual void Init(EnemyStat st)
        {
            stat = st;
        }

        public virtual void SetPaused(bool paused)
        {
            if (IsServer)
            {
                isPaused.Value = paused;
            }
        }

        public virtual void OnDied()
        {
            if (IsServer)
            {
                Died.Invoke();
            }
        }

        public virtual void OnReset() { }
    }
}

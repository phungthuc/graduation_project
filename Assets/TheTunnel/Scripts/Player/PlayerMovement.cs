using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheTunnel.Scripts.Player
{
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private Renderer playerRenderer;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsOwner) return;

            // Cache the renderer reference
            playerRenderer = GetComponent<Renderer>();
            if (playerRenderer != null)
            {
                playerRenderer.material.color = Color.red;
            }
        }

        void Update()
        {
            if (!IsOwner) return;

            float moveX = 0f;
            if (Keyboard.current.aKey.isPressed) moveX = -1f;
            else if (Keyboard.current.dKey.isPressed) moveX = 1f;

            float moveZ = 0f;
            if (Keyboard.current.wKey.isPressed) moveZ = 1f;
            else if (Keyboard.current.sKey.isPressed) moveZ = -1f;

            Vector3 move = new Vector3(moveX, 0, moveZ);
            transform.Translate(move * moveSpeed * Time.deltaTime);
        }
    }
}
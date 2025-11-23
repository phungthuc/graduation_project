using UnityEngine;
using Unity.Netcode;

namespace cowsins
{
    public class CameraFOVManager : MonoBehaviour
    {
        [SerializeField] private Rigidbody player;

        private float baseFOV;
        private Camera cam;
        private PlayerMovement movement;
        private WeaponController weapon;
        private NetworkBehaviour networkBehaviour;

        private void Start()
        {
            cam = GetComponent<Camera>();
            movement = player.GetComponent<PlayerMovement>();
            weapon = player.GetComponent<WeaponController>();
            networkBehaviour = player.GetComponent<NetworkBehaviour>();

            baseFOV = movement.normalFOV; // Initialize baseFOV once in Start
        }

        private void Update()
        {
            // Chỉ xử lý FOV cho owner, và đảm bảo camera của remote player luôn disabled
            if (networkBehaviour != null && !networkBehaviour.IsOwner)
            {
                // Đảm bảo camera của remote player luôn bị disable
                if (cam != null && cam.enabled)
                {
                    cam.enabled = false;
                    cam.cullingMask = 0;
                }
                return;
            }

            if (weapon.isAiming && weapon.weapon != null)
                return; // Not applicable if aiming

            float targetFOV;

            if (movement.wallRunning && movement.canWallRun)
            {
                targetFOV = movement.wallrunningFOV;
            }
            else if (movement.currentSpeed > movement.walkSpeed && player.linearVelocity.magnitude > 0.2f)
            {
                targetFOV = movement.runningFOV;
            }
            else
            {
                targetFOV = baseFOV;
            }

            // Smoothly interpolate FOV towards the target value
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * movement.fadeInFOVAmount);
        }
    }
}

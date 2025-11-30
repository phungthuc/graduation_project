using UnityEngine;
using Unity.Netcode;

namespace TheTunnel.UI
{
    /// <summary>
    /// Script để quay health bar về phía local player camera trong multiplayer
    /// Mỗi client sẽ thấy health bar quay về camera của chính họ
    /// </summary>
    public class HealthBarBillboard : MonoBehaviour
    {
        private Camera localPlayerCamera;
        private Transform cameraTransform;

        private void Start()
        {
            FindLocalPlayerCamera();
        }

        private void Update()
        {
            // Tìm lại camera nếu chưa có hoặc camera bị destroy
            if (cameraTransform == null || localPlayerCamera == null)
            {
                FindLocalPlayerCamera();
            }

            // Quay health bar về phía camera
            if (cameraTransform != null)
            {
                // Sử dụng LookAt để quay về camera, nhưng chỉ quay theo trục Y (horizontal)
                Vector3 direction = cameraTransform.position - transform.position;
                direction.y = 0; // Chỉ quay theo trục Y (horizontal)
                
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(-direction);
                }
            }
        }

        private void FindLocalPlayerCamera()
        {
            // Tìm local player (player của client hiện tại)
            // Trong multiplayer, mỗi client có một player object riêng
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            
            foreach (GameObject player in players)
            {
                // Kiểm tra xem player này có phải là local player không
                NetworkObject networkObject = player.GetComponent<NetworkObject>();
                if (networkObject != null && networkObject.IsOwner)
                {
                    // Tìm camera trong player object
                    // Camera thường nằm trong: Player -> CameraPivot -> CameraContainer -> Main Camera
                    Transform cameraPivot = player.transform.Find("CameraPivot");
                    if (cameraPivot != null)
                    {
                        Transform cameraContainer = cameraPivot.Find("CameraContainer");
                        if (cameraContainer != null)
                        {
                            Transform mainCameraTransform = cameraContainer.Find("Main Camera");
                            if (mainCameraTransform != null)
                            {
                                localPlayerCamera = mainCameraTransform.GetComponent<Camera>();
                                if (localPlayerCamera != null)
                                {
                                    cameraTransform = mainCameraTransform;
                                    return;
                                }
                            }
                        }
                    }

                    // Nếu không tìm thấy theo đường dẫn trên, thử tìm Camera component trực tiếp
                    Camera[] cameras = player.GetComponentsInChildren<Camera>(true);
                    foreach (Camera cam in cameras)
                    {
                        if (cam.enabled && cam.gameObject.activeInHierarchy)
                        {
                            localPlayerCamera = cam;
                            cameraTransform = cam.transform;
                            return;
                        }
                    }
                }
            }

            // Fallback: Nếu không tìm thấy local player camera, sử dụng Camera.main
            if (localPlayerCamera == null && Camera.main != null)
            {
                localPlayerCamera = Camera.main;
                cameraTransform = Camera.main.transform;
            }
        }
    }
}


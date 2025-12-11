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

        private float _lastCameraCheckTime = 0f;
        private const float CAMERA_CHECK_INTERVAL = 1f; // Check lại camera mỗi 1 giây

        private void Update()
        {
            // Tìm lại camera nếu chưa có hoặc camera bị destroy (với interval để tránh check quá nhiều)
            if (cameraTransform == null || localPlayerCamera == null || Time.time - _lastCameraCheckTime > CAMERA_CHECK_INTERVAL)
            {
                FindLocalPlayerCamera();
                _lastCameraCheckTime = Time.time;
            }

            // Quay health bar về phía camera
            if (cameraTransform != null && localPlayerCamera != null)
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
            if (NetworkManager.Singleton == null)
            {
                // Fallback nếu không có NetworkManager
                if (Camera.main != null)
                {
                    localPlayerCamera = Camera.main;
                    cameraTransform = Camera.main.transform;
                }
                return;
            }

            // Tìm local player qua NetworkManager
            if (NetworkManager.Singleton.LocalClient != null &&
                NetworkManager.Singleton.LocalClient.PlayerObject != null)
            {
                GameObject localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;

                // Cách 1: Tìm camera qua WeaponController (cách tốt nhất)
                var weaponController = localPlayer.GetComponent<cowsins.WeaponController>();
                if (weaponController != null && weaponController.mainCamera != null)
                {
                    localPlayerCamera = weaponController.mainCamera;
                    cameraTransform = weaponController.mainCamera.transform;
                    return;
                }

                // Cách 2: Tìm camera trong player object
                // Camera thường nằm trong: Player -> CameraPivot -> CameraContainer -> Main Camera
                Transform cameraPivot = localPlayer.transform.Find("CameraPivot");
                if (cameraPivot != null)
                {
                    Transform cameraContainer = cameraPivot.Find("CameraContainer");
                    if (cameraContainer != null)
                    {
                        Transform mainCameraTransform = cameraContainer.Find("Main Camera");
                        if (mainCameraTransform != null)
                        {
                            localPlayerCamera = mainCameraTransform.GetComponent<Camera>();
                            if (localPlayerCamera != null && localPlayerCamera.enabled)
                            {
                                cameraTransform = mainCameraTransform;
                                return;
                            }
                        }
                    }
                }

                // Cách 3: Tìm tất cả Camera components trong player và chọn Main Camera
                Camera[] cameras = localPlayer.GetComponentsInChildren<Camera>(true);
                foreach (Camera cam in cameras)
                {
                    // Ưu tiên camera có tag "MainCamera" hoặc tên "Main Camera"
                    if (cam.enabled && cam.gameObject.activeInHierarchy)
                    {
                        if (cam.CompareTag("MainCamera") || cam.name.Contains("Main Camera"))
                        {
                            localPlayerCamera = cam;
                            cameraTransform = cam.transform;
                            return;
                        }
                    }
                }

                // Cách 4: Lấy camera đầu tiên enabled
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

            // Fallback: Tìm bằng tag "Player" (cho trường hợp không có NetworkManager)
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                NetworkObject networkObject = player.GetComponent<NetworkObject>();
                if (networkObject != null && networkObject.IsOwner)
                {
                    var weaponController = player.GetComponent<cowsins.WeaponController>();
                    if (weaponController != null && weaponController.mainCamera != null)
                    {
                        localPlayerCamera = weaponController.mainCamera;
                        cameraTransform = weaponController.mainCamera.transform;
                        return;
                    }

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

            // Fallback cuối cùng: Sử dụng Camera.main
            if (localPlayerCamera == null && Camera.main != null)
            {
                localPlayerCamera = Camera.main;
                cameraTransform = Camera.main.transform;
                Debug.Log($"[HealthBarBillboard] Using Camera.main as fallback for {gameObject.name}");
            }
            else if (localPlayerCamera == null)
            {
                Debug.LogWarning($"[HealthBarBillboard] Could not find local player camera for {gameObject.name}. Health bar will not rotate correctly.");
            }
        }
    }
}


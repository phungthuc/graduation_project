/// <summary>
/// This script belongs to cowsins as a part of the cowsins FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
using Unity.Netcode;

namespace cowsins
{
    public class LookAt : MonoBehaviour
    {
        private Transform Player;
        private Camera localPlayerCamera;
        private Transform cameraTransform;

        private void Start()
        {
            FindLocalPlayerCamera();
            // Fallback: Tìm player transform nếu không tìm thấy camera
            if (Player == null)
            {
                Player = GameObject.FindGameObjectWithTag("Player")?.transform;
            }
        }

        private void Update()
        {
            // Ưu tiên sử dụng camera của local player
            if (cameraTransform != null && localPlayerCamera != null)
            {
                // Quay về camera, nhưng chỉ quay theo trục Y (horizontal)
                Vector3 direction = cameraTransform.position - transform.position;
                direction.y = 0; // Chỉ quay theo trục Y (horizontal)

                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(-direction);
                }
            }
            else if (Player != null)
            {
                // Fallback: Quay về player position (giữ nguyên logic cũ)
                transform.LookAt(new Vector3(Player.position.x, transform.position.y, Player.position.z));
            }
            else
            {
                // Tìm lại camera nếu chưa có
                FindLocalPlayerCamera();
            }
        }

        private void FindLocalPlayerCamera()
        {
            // Tìm local player (player của client hiện tại)
            // Ưu tiên sử dụng NetworkManager để tìm local player
            if (NetworkManager.Singleton != null)
            {
                // Tìm local player qua NetworkManager
                if (NetworkManager.Singleton.LocalClient != null &&
                    NetworkManager.Singleton.LocalClient.PlayerObject != null)
                {
                    GameObject localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
                    Player = localPlayer.transform; // Set player transform làm fallback

                    // Cách 1: Tìm camera qua WeaponController (cách tốt nhất)
                    var weaponController = localPlayer.GetComponent<WeaponController>();
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
            }

            // Fallback: Tìm bằng tag "Player" (cho trường hợp không có NetworkManager)
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                NetworkObject networkObject = player.GetComponent<NetworkObject>();
                if (networkObject != null && networkObject.IsOwner)
                {
                    Player = player.transform; // Set player transform làm fallback

                    var weaponController = player.GetComponent<WeaponController>();
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

            // Fallback cuối cùng: Sử dụng Camera.main hoặc tìm player transform
            if (localPlayerCamera == null && Camera.main != null)
            {
                localPlayerCamera = Camera.main;
                cameraTransform = Camera.main.transform;
            }

            // Fallback: Tìm player transform nếu chưa có
            if (Player == null)
            {
                Player = GameObject.FindGameObjectWithTag("Player")?.transform;
            }
        }
    }
}

/// <summary>
/// This script belongs to cowsins� as a part of the cowsins� FPS Engine. All rights reserved. 
/// </summary>

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using TMPro;
using Unity.Netcode;
namespace cowsins
{
    public class GetGameInformation : MonoBehaviour
    {
        //FPS
        public bool showFPS;

        public bool showMinimumFrameRate, showMaximumFrameRate;

        [SerializeField, Range(.01f, 1f)] private float fpsRefreshRate;

        [SerializeField] private TextMeshProUGUI fpsObject;

        [SerializeField] private Color appropriateValueColor, intermediateValueColor, badValueColor;

        //Ping
        public bool showPing;

        [SerializeField, Range(.01f, 1f)] private float pingRefreshRate;

        [SerializeField] private Color goodPingColor, mediumPingColor, badPingColor;

        private float fpsTimer;
        private float pingTimer;

        private float fps, minFps, maxFps;
        private float currentPing;

        private string text = "";

        private void Start()
        {
            if (showFPS)
                fpsTimer = fpsRefreshRate;
            else
                Destroy(fpsObject);

            if (showPing)
                pingTimer = pingRefreshRate;

            minFps = float.MaxValue;
        }

        private void Update()
        {
            if (!showFPS) return;

            fpsTimer -= Time.deltaTime;
            if (showPing)
                pingTimer -= Time.deltaTime;

            if (fpsTimer <= 0)
            {
                text = "";
                fps = 1.0f / Time.deltaTime;

                if (fps < minFps) minFps = fps;
                if (fps > maxFps) maxFps = fps;

                fpsTimer = fpsRefreshRate;

                text += "Current FPS: " + GetColoredFPSText(fps) + "\n";

                if (showMinimumFrameRate)
                    text += "Min FPS: " + GetColoredFPSText(minFps) + "\n";

                if (showMaximumFrameRate)
                    text += "Max FPS: " + GetColoredFPSText(maxFps);

                if (showPing && pingTimer <= 0)
                {
                    UpdatePing();
                    pingTimer = pingRefreshRate;
                }

                if (showPing)
                {
                    text += "\nPing: " + GetColoredPingText(currentPing);
                }

                fpsObject.text = text;
            }

        }

        private string GetColoredFPSText(float fps)
        {
            Color fpsColor;

            if (fps < 15f)
            {
                fpsColor = badValueColor;
            }
            else if (fps < 45f)
            {
                fpsColor = intermediateValueColor;
            }
            else
            {
                fpsColor = appropriateValueColor;
            }

            return "<color=#" + ColorUtility.ToHtmlStringRGB(fpsColor) + ">" + fps.ToString("F0") + "</color>";
        }

        /// <summary>
        /// Lấy ping (RTT) từ NetworkManager
        /// </summary>
        private void UpdatePing()
        {
            if (NetworkManager.Singleton == null)
            {
                currentPing = 0f;
                return;
            }

            // Host có ping rất thấp (local)
            if (NetworkManager.Singleton.IsHost)
            {
                currentPing = 1f;
                return;
            }

            // Chỉ client mới cần tính ping thực tế
            if (!NetworkManager.Singleton.IsClient)
            {
                currentPing = 0f;
                return;
            }

            // Lấy RTT từ NetworkTransport trước
            try
            {
                if (NetworkManager.Singleton.NetworkConfig != null &&
                    NetworkManager.Singleton.NetworkConfig.NetworkTransport != null)
                {
                    var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport;
                    currentPing = GetRTTFromTransport(transport);
                }

                // Nếu không lấy được từ transport, tính từ NetworkTimeSystem
                if (currentPing <= 0f)
                {
                    var networkTimeSystem = NetworkManager.Singleton.NetworkTimeSystem;
                    if (networkTimeSystem != null)
                    {
                        // Cách 1: Thử truy cập RTT property trực tiếp
                        var timeSystemType = networkTimeSystem.GetType();
                        var rttProperty = timeSystemType.GetProperty("RTT",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) ??
                                         timeSystemType.GetProperty("LocalRTT",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) ??
                                         timeSystemType.GetProperty("CurrentRtt",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                        if (rttProperty != null)
                        {
                            var rtt = rttProperty.GetValue(networkTimeSystem);
                            if (rtt != null)
                            {
                                float rttValue = System.Convert.ToSingle(rtt);
                                if (rttValue < 1f && rttValue > 0f)
                                {
                                    currentPing = rttValue * 1000f;
                                }
                                else
                                {
                                    currentPing = rttValue;
                                }
                            }
                        }

                        // Cách 2: Tính toán RTT từ LocalTime và ServerTime (fallback)
                        // NetworkTimeSystem sử dụng RTT để sync time, nên có thể tính ngược lại
                        if (currentPing <= 0f)
                        {
                            try
                            {
                                // Thử truy cập LocalRTT qua reflection (có thể là private)
                                var localRTTField = timeSystemType.GetField("LocalRTT",
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) ??
                                                   timeSystemType.GetField("m_LocalRTT",
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) ??
                                                   timeSystemType.GetField("_localRTT",
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                                if (localRTTField != null)
                                {
                                    var rtt = localRTTField.GetValue(networkTimeSystem);
                                    if (rtt != null)
                                    {
                                        float rttValue = System.Convert.ToSingle(rtt);
                                        // RTT có thể là seconds, chuyển sang milliseconds
                                        if (rttValue < 1f && rttValue > 0f)
                                        {
                                            currentPing = rttValue * 1000f;
                                        }
                                        else
                                        {
                                            currentPing = rttValue;
                                        }
                                    }
                                }

                                // Nếu vẫn không có, thử tính từ time difference
                                // Lưu ý: Cách này không chính xác lắm nhưng có thể dùng được
                                if (currentPing <= 0f)
                                {
                                    var localTime = networkTimeSystem.LocalTime;
                                    var serverTime = networkTimeSystem.ServerTime;
                                    var timeDiff = System.Math.Abs((double)(localTime - serverTime));

                                    // RTT được sử dụng để sync time, nên timeDiff ≈ RTT/2
                                    // Do đó RTT ≈ timeDiff * 2
                                    // Chuyển từ seconds sang milliseconds
                                    if (timeDiff > 0.0001)
                                    {
                                        currentPing = (float)(timeDiff * 2000.0);
                                    }
                                }
                            }
                            catch (System.Exception ex)
                            {
                                // Log để debug
                                Debug.LogWarning($"[GetGameInformation] Failed to calculate RTT from NetworkTimeSystem: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Log lỗi để debug (có thể comment lại sau khi test)
                Debug.LogWarning($"[GetGameInformation] Failed to get ping: {ex.Message}");
                currentPing = 0f;
            }
        }

        /// <summary>
        /// Lấy RTT từ NetworkTransport (Unity Transport Package)
        /// </summary>
        private float GetRTTFromTransport(Unity.Netcode.Transports.UTP.UnityTransport transport)
        {
            try
            {
                // Thử gọi GetCurrentRtt trực tiếp trên UnityTransport
                // Method này có thể có signature: GetCurrentRtt(ulong clientId)
                var transportType = transport.GetType();

                // Thử tìm method GetCurrentRtt với các signature khác nhau
                var getRTTMethod = transportType.GetMethod("GetCurrentRtt",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                    null,
                    new System.Type[] { typeof(ulong) },
                    null);

                if (getRTTMethod != null)
                {
                    // Gọi với LocalClientId để lấy RTT của client hiện tại đến server
                    ulong clientId = NetworkManager.Singleton.LocalClientId;
                    var rtt = getRTTMethod.Invoke(transport, new object[] { clientId });

                    if (rtt != null)
                    {
                        // RTT thường trả về int (milliseconds) hoặc float
                        float rttValue = System.Convert.ToSingle(rtt);
                        return rttValue;
                    }
                }

                // Fallback: Thử truy cập qua NetworkDriver nếu GetCurrentRtt không có
                var driverField = transportType.GetField("m_Driver",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) ??
                                  transportType.GetField("driver",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) ??
                                  transportType.GetField("_driver",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (driverField != null)
                {
                    var driver = driverField.GetValue(transport);
                    if (driver != null)
                    {
                        var driverType = driver.GetType();

                        // Thử tìm GetRTT hoặc GetCurrentRtt trên driver
                        var driverGetRTTMethod = driverType.GetMethod("GetRTT",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) ??
                                          driverType.GetMethod("GetCurrentRtt",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                        if (driverGetRTTMethod != null)
                        {
                            // Thử với ServerClientId (0) hoặc LocalClientId
                            ulong clientId = NetworkManager.Singleton.IsClient ?
                                NetworkManager.Singleton.LocalClientId : NetworkManager.ServerClientId;

                            var rtt = driverGetRTTMethod.Invoke(driver, new object[] { clientId });
                            if (rtt != null)
                            {
                                float rttValue = System.Convert.ToSingle(rtt);
                                // RTT có thể là milliseconds hoặc seconds
                                if (rttValue < 1f && rttValue > 0f)
                                {
                                    return rttValue * 1000f;
                                }
                                return rttValue;
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Log lỗi để debug
                Debug.LogWarning($"Failed to get RTT from UnityTransport: {ex.Message}");
            }

            return 0f;
        }

        /// <summary>
        /// Lấy RTT từ NetworkTransport (generic method cho các transport khác)
        /// </summary>
        private float GetRTTFromTransport(object transport)
        {
            // Thử cast về UnityTransport
            if (transport is Unity.Netcode.Transports.UTP.UnityTransport utpTransport)
            {
                return GetRTTFromTransport(utpTransport);
            }

            // Thử các cách khác để lấy RTT cho các transport khác
            try
            {
                var transportType = transport.GetType();

                // Thử tìm method GetCurrentRtt với signature (ulong clientId)
                var getRTTMethod = transportType.GetMethod("GetCurrentRtt",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                    null,
                    new System.Type[] { typeof(ulong) },
                    null) ??
                                   transportType.GetMethod("GetRTT",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                    null,
                    new System.Type[] { typeof(ulong) },
                    null);

                if (getRTTMethod != null)
                {
                    // Sử dụng LocalClientId cho client, ServerClientId cho server
                    ulong clientId = NetworkManager.Singleton.IsClient ?
                        NetworkManager.Singleton.LocalClientId : NetworkManager.ServerClientId;

                    var rtt = getRTTMethod.Invoke(transport, new object[] { clientId });
                    if (rtt != null)
                    {
                        float rttValue = System.Convert.ToSingle(rtt);
                        // RTT thường là milliseconds, nhưng kiểm tra để chắc chắn
                        if (rttValue < 1f && rttValue > 0f)
                        {
                            return rttValue * 1000f;
                        }
                        return rttValue;
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Log lỗi để debug
                Debug.LogWarning($"Failed to get RTT from transport {transport.GetType().Name}: {ex.Message}");
            }

            return 0f;
        }

        private string GetColoredPingText(float ping)
        {
            Color pingColor;

            if (ping <= 0f)
            {
                return "<color=#" + ColorUtility.ToHtmlStringRGB(mediumPingColor) + ">N/A</color>";
            }
            else if (ping < 50f)
            {
                pingColor = goodPingColor;
            }
            else if (ping < 100f)
            {
                pingColor = mediumPingColor;
            }
            else
            {
                pingColor = badPingColor;
            }

            return "<color=#" + ColorUtility.ToHtmlStringRGB(pingColor) + ">" + ping.ToString("F0") + " ms</color>";
        }
    }
#if UNITY_EDITOR
    [System.Serializable]
    [CustomEditor(typeof(GetGameInformation))]
    public class GetGameInformatioEditor : Editor
    {

        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            GetGameInformation myScript = target as GetGameInformation;

            EditorGUILayout.LabelField("FPS", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showFPS"));
            if (myScript.showFPS)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fpsRefreshRate"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fpsObject"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("showMinimumFrameRate"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("showMaximumFrameRate"));
            }
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("PING", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showPing"));
            if (myScript.showPing)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pingRefreshRate"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("goodPingColor"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("mediumPingColor"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("badPingColor"));
            }
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("COLOR", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("appropriateValueColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intermediateValueColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("badValueColor"));

            serializedObject.ApplyModifiedProperties();

        }
    }
#endif
}
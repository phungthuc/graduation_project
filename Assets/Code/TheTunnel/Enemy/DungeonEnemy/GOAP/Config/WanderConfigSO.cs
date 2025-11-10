using UnityEngine;

namespace TheTunnel.GOAP
{
    [CreateAssetMenu(menuName = "AI/Wander Config", fileName = "Wander Config", order = 1)]
    public class WanderConfigSO : ScriptableObject
    {
        public Vector2 waitRangeBetweenWander = new(1, 5);
        public float wanderRadius = 5;
    }
}
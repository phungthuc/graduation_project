using CrashKonijn.Goap.Classes.References;
using CrashKonijn.Goap.Interfaces;
using UnityEngine;

namespace TheTunnel.GOAP
{
    public class CommonData : IActionData
    {
        public ITarget Target { get; set; }
        public float timer { get; set; }

        [GetComponentInChildren]
        public Animator Animator { get; set; }

        public static readonly int Attack = Animator.StringToHash("attack");
        public static readonly int SpiderJumpToPlayer = Animator.StringToHash("jumpToPlayer");
        public static readonly int Running = Animator.StringToHash("isRunning");
        public static readonly int Walking = Animator.StringToHash("isWalking");
        public static readonly int Aiming = Animator.StringToHash("isAiming");
        public static readonly int Reloading = Animator.StringToHash("isReloading");
        public static readonly int Appearing = Animator.StringToHash("isAppear");

    }
}
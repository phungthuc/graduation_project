using System;
using UnityEngine;

namespace TheTunnel.CharacterControl
{
    public class CharacterStateManager : MonoBehaviour
    {
        public bool IsPaused { get; set; } = false;

        public Animator animator;

        public ICharacterState currentState;

        public virtual void Start() { }

        public void Update()
        {
            if (currentState == null || IsPaused)
            {
                return;
            }
            currentState.Execute(this);
        }

        public void ChangeState(ICharacterState newState)
        {
            if (currentState != null)
            {
                currentState.Exit(this);
            }
            currentState = newState;
            currentState.Enter(this);
        }
    }
}
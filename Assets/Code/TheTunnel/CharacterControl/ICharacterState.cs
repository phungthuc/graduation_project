using System;
using UnityEngine;

namespace TheTunnel.CharacterControl
{
    public interface ICharacterState 
    {
        public void Enter(CharacterStateManager characterStateManager);
        
        public void Exit(CharacterStateManager characterStateManager);
        
        public void Execute(CharacterStateManager characterStateManager);   
    }
}
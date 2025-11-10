using cowsins;
using UnityEngine;

namespace TheTunnel.Custom.cowsins
{
    public class Custom_PlayerStates : PlayerStates
    {
       public override void Awake()
       {
           _instance = this;

           _states = new Custom_PlayerStateFactory(this);
;          _currentState = _states.Default();
           _currentState.EnterState();
       }
    }
}
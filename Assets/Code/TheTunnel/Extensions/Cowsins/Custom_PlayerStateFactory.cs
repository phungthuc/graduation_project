using cowsins;
using UnityEngine;

namespace TheTunnel.Custom.cowsins
{
    public class Custom_PlayerStateFactory : PlayerStateFactory
    {
        public Custom_PlayerStateFactory(PlayerStates currentContext) : base(currentContext)
        {
            _context = currentContext;
        }
        
        public override PlayerBaseState Default() { return new Custom_PlayerDefaultState(_context, this); }
    }
}
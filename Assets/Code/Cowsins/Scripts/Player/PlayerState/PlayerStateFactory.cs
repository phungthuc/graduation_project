namespace cowsins
{
    public class PlayerStateFactory
    {
        protected PlayerStates _context;

        public PlayerStateFactory(PlayerStates currentContext) { _context = currentContext; }

        public virtual PlayerBaseState Default() { return new PlayerDefaultState(_context, this); }

        public virtual PlayerBaseState Jump() { return new PlayerJumpState(_context, this); }

        public virtual PlayerBaseState Crouch() { return new PlayerCrouchState(_context, this); }

        public virtual PlayerBaseState Die() { return new PlayerDeadState(_context, this); }

        public virtual PlayerBaseState Dash() { return new PlayerDashState(_context, this, new UnityEngine.Vector3(InputManager.x, 0, InputManager.y)); }

        public virtual PlayerBaseState Climb() { return new PlayerClimbState(_context, this); }
    }
}
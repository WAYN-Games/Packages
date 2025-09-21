using Wayn.Locomotion.StateMachine;

namespace WAYN.Locomotion.Demo.BasicLocomotion
{
    
    [LocomotionStateMachine(typeof(BasicLocomotionContext))]
    public enum BasicLocomotionStates : byte
    {
        [LocomotionState(typeof(FallingState))] Falling,
        [LocomotionState(typeof(MovingState))] Moving,
        [LocomotionState(typeof(SlidingState))] Sliding,
        [LocomotionState(typeof(IdleState))] Idle, 
        [LocomotionState(typeof(JumpingState))] Jumping
    }

    public struct JumpingState
    {
    }

    public struct IdleState
    {
    }

    public struct SlidingState
    {
    }

    public struct MovingState
    {
    }

    public struct FallingState
    {
    }

    public struct BasicLocomotionContext
    {
    }
}
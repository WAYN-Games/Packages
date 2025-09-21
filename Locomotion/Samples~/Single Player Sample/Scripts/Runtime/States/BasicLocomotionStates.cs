using Wayn.Locomotion.StateMachine;

namespace WAYN.Locomotion.Demo.BasicLocomotion
{
    /// <summary>
    /// Represents the different possible locomotion states for basic movement in a character controller system.
    /// This enumeration is used within the locomotion state machine to determine the active state of an entity
    /// and manage transitions between them.
    /// </summary>
    /// <remarks>
    /// Each state is associated with a specific implementation of the <see cref="ILocomotionState{TContext, TProfile, TInput, TStateData}"/>
    /// interface, defining the behavior and logic of the locomotion process.
    /// </remarks>
    [LocomotionStateMachine(typeof(BasicLocomotionContext),
        typeof(BasicLocomotionProfile),
        typeof(BasicLocomotionInput),
        typeof(BasicLocomotionStateData))]
    public enum BasicLocomotionStates : byte
    {
        [LocomotionState(typeof(FallingState))] Falling,
        [LocomotionState(typeof(MovingState))] Moving,
        [LocomotionState(typeof(SlidingState))] Sliding,
        [LocomotionState(typeof(IdleState))] Idle,
        [LocomotionState(typeof(JumpingState))] Jumping
    }
}
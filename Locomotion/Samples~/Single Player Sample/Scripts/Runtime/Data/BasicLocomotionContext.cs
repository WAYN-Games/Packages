using Unity.Mathematics;
using Wayn.Locomotion.StateMachine;

namespace WAYN.Locomotion.Demo.BasicLocomotion
{
    /// <summary>
    /// Represents the context for basic locomotion mechanics, providing access to profile data,
    /// input data, state data, and base locomotion operations. This is primarily used in the
    /// state machine system for character movement and physics interactions.
    /// </summary>
    public struct BasicLocomotionContext : ILocomotionContext<BasicLocomotionProfile, BasicLocomotionInput,
        BasicLocomotionStateData>
    {


        // Explicit properties forced by interface
        // These return a value copy of the data
        // meaning they can't be modified without reassignment.
        // This isn't and issue for Profile value is configuration
        // data and isn't supposed to be modified. 
        // For Input you might need to "consume" the input in cases such as 
        // jumping or other trigger input kind.
        // In such case you need to do something like :
        // var inputValue = BasicLocomotionContext.InputValue;
        // inputValue.Trigger = 0;
        // BasicLocomotionContext.InputValue = inputValue;
        
        public BasicLocomotionProfile ProfileValue { get; set; }
        public BasicLocomotionInput InputValue { get; set; }
        
        // Alternatively, in cases where you frequently need to update the data
        // such as the state data or context, you can use a backing field instead
        // of the simple auto property forced by teh interface.
        // In that case these are still returning value copies and should be used
        // as explained in hte Input example above.
        
        public BasicLocomotionStateData StateDataValue
        {
            get => StateData;
            set => StateData = value;
        }
        public BaseLocomotionContext BaseContextValue
        {
            get => BaseContext;
            set => BaseContext = value;
        }
        
        // But you could also use the backing field by ref to avoid the need for reassignment :
        // var ref context = ref BasicLocomotionContext.BaseContext
        // context.VerticalVelocity = float3.zero;
        
        public BaseLocomotionContext BaseContext;
        public BasicLocomotionStateData StateData; 

        
        public void ComputeGroundSnappingDistance(in BasicLocomotionProfile profile, out float distance)
        {
            var slopeDownDistance = math.length(BaseContext.HorizontalVelocity * BaseContext.TimeStep) *
                                    math.tan(math.radians(ProfileValue.Moving.MaxSlope));
            distance = math.max(ProfileValue.Moving.MaxStepHeight, 2 * slopeDownDistance) +
                       ProfileValue.PhysicsProfileValue.ShellWidth;
        }


        /// <summary>
        /// Evaluates whether the current contact could potentially be a step and calculates the step offset if applicable.
        /// </summary>
        /// <param name="stepOffset">The calculated offset for the potential step, if valid. It is set to zero if a step is not possible.</param>
        /// <returns>Returns true if the contact is a step; otherwise, returns false.</returns>
        public bool CouldBeAStep(out float3 stepOffset)
        {
            stepOffset = float3.zero;
            if (!IsSlopeAboveMax) return false;
            if (!BaseContext.IsMovingAgainstSlope) return false;
            stepOffset = BaseContext.StepHeight(ProfileValue.PhysicsProfileValue.ShellWidth);
            return math.lengthsq(stepOffset) <= math.square(ProfileValue.Moving.MaxStepHeight);
        }


        /// <summary>
        /// Determines whether the slope angle of the surface the entity is in contact with exceeds the maximum allowable slope.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the slope angle exceeds or equals the maximum allowed slope; otherwise, <c>false</c>.
        /// </returns>
        public bool IsSlopeAboveMax
        {
            get
            {
                MathematicsHelpers.CalculateSlopeAngle(out var angle,
                    BaseContextValue.LocomotionContact.ValueRO.Contact.Normal,
                    -BaseContextValue.GravityDirection);
                return angle >=
                       math.radians(ProfileValue.Moving.MaxSlope);
            }
        }

        /// <summary>
        /// Indicates whether the entity is in contact with a vertical or near-vertical surface that can be considered a wall.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the surface behaves as a wall based on the slope and ceiling criteria; otherwise, <c>false</c>.
        /// </returns>
        public bool IsWall => !BaseContextValue.IsCeiling && IsSlopeAboveMax;

        /// <summary>
        /// Indicates whether the entity intends to move based on the magnitude of the movement input.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the movement magnitude exceeds the defined threshold; otherwise, <c>false</c>.
        /// </returns>
        public bool WantsToMove => InputValue.MovementMagnitude > math.EPSILON;
    }
}
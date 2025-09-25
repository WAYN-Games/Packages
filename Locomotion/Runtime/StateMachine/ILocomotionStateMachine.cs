// /*
//  * Copyright (c) 2025 WAYN Games.
//  *
//  * All rights reserved. This software and associated documentation files
//  * (the "Software") are the exclusive property of WAYN Games.
//  * Unauthorized copying of this file, via any medium, is strictly prohibited.
//  *
//  * This software may not be reproduced, distributed, or used in any manner
//  * whatsoever without the express written permission of the author, except for
//  * the use of brief quotations in a review or other non-commercial uses permitted
//  * by copyright law.
//  *
//  * For permissions, contact: contact@wayn.games
//  */

using WAYNGames.Locomotion.Runtime.Components;
using Unity.Entities;
using Unity.Profiling;

namespace WAYNGames.Locomotion.Runtime.StateMachine
{
    /// <summary>
    /// Interface for defining locomotion states within a character movement state machine.
    /// </summary>
    /// <typeparam name="TContext">
    /// The type of the context that encapsulates necessary state, input, and profile data required for locomotion.
    /// </typeparam>
    /// <typeparam name="TProfile">
    /// The type of the locomotion profile which defines specific configuration data for the locomotion system.
    /// </typeparam>
    /// <typeparam name="TInput">
    /// The type of the input data used to control and influence the locomotion system.
    /// </typeparam>
    /// <typeparam name="TStateData">
    /// The type of the state data that holds transitional and dynamic state information for locomotion.
    /// </typeparam>
    public interface ILocomotionState<TContext, TProfile, TInput, TStateData>
        where TContext : unmanaged, ILocomotionContext<TProfile, TInput, TStateData>
        where TProfile : unmanaged, ILocomotionProfile
        where TInput : unmanaged, IComponentData
        where TStateData : unmanaged, IComponentData
    {
        /// <summary>
        /// Method invoked when entering a new locomotion state within the state machine.
        /// </summary>
        /// <param name="ctx">
        /// The context of the locomotion system, containing state, input, and profile data required for the state transition.
        /// </param>
        public static void OnEnterState(ref TContext ctx)
        {
        }

        /// <summary>
        /// Method invoked to integrate the current state within the locomotion system, allowing for processing of inputs and computation of the displacement.
        /// </summary>
        /// <param name="ctx">
        /// The context of the locomotion system, containing state, input, and profile data necessary for the integration process.
        /// </param>
        public static void OnIntegrateState(ref TContext ctx)
        {
        }

        /// <summary>
        /// Method to move the character based on the displacement computed during integration.
        /// This method represents one iteration of the "Collide and Slide" algorithm to resolve collision.
        /// After each move, the displacement should be updated to reflect the portion of displacement that could not be achieved due to a collision.
        /// This method will be re invoked, up to <see cref="CharacterPhysicsProfile"/>.<c>MaxBounce</c> times, as long as the displacements in the context aren't set to 0.  
        /// </summary>
        /// <param name="ctx">
        /// The context of the locomotion system, containing state, input, and profile data relevant for handling collisions.
        /// </param>
        public static void OnResolveStateCollisions(ref TContext ctx)
        {
        }

        /// <summary>
        /// Method invoked when exiting the current locomotion state within the state machine.
        /// </summary>
        /// <param name="ctx">
        /// The context of the locomotion system, containing state, input, and profile data relevant to the state being exited.
        /// </param>
        static void OnExitState(ref TContext ctx)
        {
        }
    }

    public interface ILocomotionProfile
    {
        /// <summary>
        /// The physics profile associated with the character.
        /// </summary>
        /// <returns>
        /// Returns a <see cref="CharacterPhysicsProfile"/> which contains the physics-related configuration for the locomotion system.
        /// </returns>
        public CharacterPhysicsProfile PhysicsProfileValue {get; set; }
    }

    /// <summary>
    /// Stores a reference to the <see cref="PlayerInputAsset{TInput}"/> instance of a player.
    /// </summary>
    /// <remarks>
    /// This is the instance of the asset specific to a player and should be used at runtime.
    /// It will contain the cached player specific data set during the <see cref="PlayerInputAsset{TInput}"/>.<c>InitInputAsset(PlayerGameObject playerGameObject)</c> method invocation.
    /// </remarks>
    /// <typeparam name="TInput">
    /// The type of the input used for character locomotion, which must be unmanaged and implement ICharacterLocomotionInput.
    /// </typeparam>
    public struct LocomotionInputInstance<TInput> : ICleanupComponentData where TInput : unmanaged, ICharacterLocomotionInput<TInput>
    {
        public UnityObjectRef<PlayerInputAsset<TInput>> Instance;
    }

    /// <summary>
    /// Stores a reference to the original <c>PlayerInputAsset</c> used by a player.
    /// </summary>
    /// <remarks>
    /// This is the editor scriptable asset and should not be used at runtime.
    /// </remarks>
    /// <typeparam name="TInput">
    /// The type of input structure implementing the ICharacterLocomotionInput interface, which defines the input data for controlling character locomotion.
    /// </typeparam>
    public struct LocomotionInputAsset<TInput> : IComponentData where TInput : unmanaged, ICharacterLocomotionInput<TInput>
    {
        public UnityObjectRef<PlayerInputAsset<TInput>> Asset;
    }


    /// <summary>
    /// Locomotion profile in the form of a blob asset.
    /// This profile contains configuration and parameters necessary for defining locomotion behavior within a state machine.
    /// </summary>
    /// <typeparam name="TProfile">
    /// The type of the locomotion profile that implements the ILocomotionProfile interface. It must be unmanaged to ensure
    /// compatibility with Unity's ECS system.
    /// </typeparam>
    public struct LocomotionProfileBlob<TProfile> : IComponentData where TProfile : unmanaged, ILocomotionProfile
    {
        public BlobAssetReference<TProfile> Profile;
    }

    /// <summary>
    /// Interface enforcing the minimal information need for the context of locomotion state machine. It enforces the encapsulation configuration, input, and state data.
    /// </summary>
    /// <typeparam name="TProfile">
    /// The type of the locomotion profile which contains configuration parameters and properties for locomotion.
    /// </typeparam>
    /// <typeparam name="TInput">
    /// The type of the input data used to control and interact with the locomotion system.
    /// </typeparam>
    /// <typeparam name="TStateData">
    /// The type of the state data representing dynamic and transitional information for locomotion.
    /// </typeparam>
    public interface ILocomotionContext<TProfile, TInput, TStateData>
        where TProfile : unmanaged, ILocomotionProfile
        where TInput : unmanaged, IComponentData
        where TStateData : unmanaged, IComponentData
    {
        
        /// <inheritdoc cref="BaseLocomotionContext"/>
        public BaseLocomotionContext BaseContextValue { get; set; }

        /// <summary>
        /// This property defines the attributes (movement speed, jump force, ...) configuring the  
        /// specific state locomotion logic execution.
        /// </summary>
        public TProfile ProfileValue { get; set; }

        /// <summary>
        /// The input data (movement direction, action triggers, ...) enabling control of the locomotion state logic by the player.
        /// </summary>
        public TInput InputValue { get; set; }

        /// <summary>
        /// The specific state data (i.e. not in <see cref="BaseLocomotionContext"/>) needed for the locomotion
        /// system's processing and state logic.
        /// </summary>
        public TStateData StateDataValue { get; set; }

        /// <summary>
        /// Method to compute the distance to check for the snapping  of the character to the ground during locomotion.
        /// </summary>
        /// <param name="profile">
        /// The locomotion profile containing parameters relevant to computing the ground snapping distance.
        /// </param>
        /// <param name="distance">
        /// The output parameter that is assigned the computed ground snapping distance.
        /// </param>
        public void ComputeGroundSnappingDistance(in TProfile profile, out float distance)
        {
            distance = profile.PhysicsProfileValue.ShellWidth;
        }
        
        
    }

    /// <summary>
    /// Interface that defines the structure of a character locomotion state machine, responsible for managing state transitions,
    /// physics integration, collision resolution, and other locomotion behaviors of characters.
    /// </summary>
    /// <typeparam name="TStateMachine">
    /// The type of the locomotion state machine itself, which implements this interface.
    /// </typeparam>
    /// <typeparam name="TStates">
    /// The type representing the collection of states managed by the state machine.
    /// </typeparam>
    /// <typeparam name="TContext">
    /// The type of the context object that provides state, input data, and profile information necessary for the locomotion logic.
    /// </typeparam>
    /// <typeparam name="TProfile">
    /// The type of the locomotion profile containing configuration settings specific to the locomotion logic.
    /// </typeparam>
    /// <typeparam name="TInput">
    /// The type of the input data influencing locomotion, such as user inputs or triggers.
    /// </typeparam>
    /// <typeparam name="TStateData">
    /// The type of the state data component holding state specific information.
    /// </typeparam>
    public interface ILocomotionStateMachine<TStateMachine, TStates, TContext, TProfile, TInput, TStateData>
        where TStateMachine : unmanaged,
        ILocomotionStateMachine<TStateMachine, TStates, TContext, TProfile, TInput, TStateData>
        where TContext : unmanaged, ILocomotionContext<TProfile, TInput, TStateData>
        where TProfile : unmanaged, ILocomotionProfile
        where TInput : unmanaged, IComponentData
        where TStateData : unmanaged, IComponentData
        where TStates : unmanaged
    {
        static readonly ProfilerMarker KStateMachineSolveOverlapsPerfMarker =
            new("ILocomotionStateMachine.SolveOverlaps");

        static readonly ProfilerMarker KStateMachineComputeInheritedDisplacementPerfMarker =
            new("ILocomotionStateMachine.ComputeInheritedDisplacement");

        static readonly ProfilerMarker KStateMachineIntegratePerfMarker = new("ILocomotionStateMachine.Integrate");

        static readonly ProfilerMarker KStateMachineResolveCollisionPerfMarker =
            new("ILocomotionStateMachine.ResolveCollision");

        static readonly ProfilerMarker KStateMachineSnapToGroundPerfMarker =
            new("ILocomotionStateMachine.SnapToGround");

        /// <summary>
        /// Collision resolution stage.
        /// This method is replaced by a source generator to implement the switch statement that will invoke the appropriate collision resolution logic depending on the current state of the state machine.
        /// </summary>
        /// <param name="context">
        /// The context encapsulating locomotion state, environmental data, and collision information.
        /// </param>
        public void ResolveCollision(ref TContext context)
        {
        }

        /// <summary>
        /// Integration stage.
        /// This method is replaced by a source generator to implement the switch statement that will invoke the appropriate integration logic depending on the current state of the state machine.
        /// </summary>
        /// <param name="context">
        /// The context encapsulating locomotion state, environmental data, and collision information.
        /// </param>
        public void Integrate(ref TContext context)
        {
        }

        /// <summary>
        /// Transition stage.
        /// This method is replaced by a source generator to implement the switch statement that will invoke the appropriate transition evaluation logic depending on the current state of the state machine.
        /// </summary>
        /// <param name="context">
        /// The current context of the locomotion system, containing state, input, and profile data utilized for transition evaluation.
        /// </param>
        public void EvaluateTransition(ref TContext context)
        {
        }


        /// <summary>
        /// Defines the default locomotion state machine logic as described in documentation "Character Movement Execution".
        /// </summary>
        /// <param name="stateMachine">
        /// Reference to the state machine controlling the locomotion system.
        /// </param>
        /// <param name="context">
        /// Reference to the context containing the state, input, profile, and physics data necessary for locomotion state execution.
        /// </param>
        public static void Execute(ref TStateMachine stateMachine, ref TContext context)
        {
            BaseLocomotionContext baseContext = context.BaseContextValue;
            TProfile locomotionProfile = context.ProfileValue;
            CharacterPhysicsProfile physicsProfile = locomotionProfile.PhysicsProfileValue;
            KStateMachineSolveOverlapsPerfMarker.Begin();
            PhysicsOperations.SolveOverlaps(ref baseContext, in physicsProfile);
            KStateMachineSolveOverlapsPerfMarker.End();


            baseContext.InitialPosition = baseContext.CharacterPosition.ValueRO.Position;

            if (baseContext.IsInContact)
            {
                KStateMachineComputeInheritedDisplacementPerfMarker.Begin();
                PhysicsOperations.ComputeInheritedDisplacement(ref baseContext);
                KStateMachineComputeInheritedDisplacementPerfMarker.End();
                context.BaseContextValue = baseContext;
            }

            KStateMachineIntegratePerfMarker.Begin();
            stateMachine.Integrate(ref context);
            KStateMachineIntegratePerfMarker.End();


            KStateMachineResolveCollisionPerfMarker.Begin();
            for (var i = 0; i < physicsProfile.MaxBounce; i++)
            {
                stateMachine.ResolveCollision(ref context);


                baseContext.InitialPosition = baseContext.CharacterPosition.ValueRO.Position;

                stateMachine.EvaluateTransition(ref context);

                if (!context.BaseContextValue.HasRemainingDisplacement) break;
            }

            KStateMachineResolveCollisionPerfMarker.End();

            if (!baseContext.IsInContact) return;
            KStateMachineSnapToGroundPerfMarker.Begin();
            context.ComputeGroundSnappingDistance(in locomotionProfile, out var distanceToCheck);

            PhysicsOperations.SnapToGround(ref baseContext, in physicsProfile, distanceToCheck);
            stateMachine.EvaluateTransition(ref context);
            KStateMachineSnapToGroundPerfMarker.End();
        }
    }
}
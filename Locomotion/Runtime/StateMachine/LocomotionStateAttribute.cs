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

using System;

namespace WAYNGames.Locomotion.Runtime.StateMachine
{
    /// <summary>
    ///     This attribute mark an enum field as a Locomotion state.
    ///     This is picked up by a source generator to generate the corresponding state machine dispatch.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LocomotionStateAttribute : Attribute
    {
        public LocomotionStateAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }

    /// <summary>
    ///     This attribute mark an enum as a Locomotion state machine.
    ///     This is picked up by a source generator to generate the corresponding state machine component and systems.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public class LocomotionStateMachineAttribute : Attribute
    {
        public LocomotionStateMachineAttribute(Type context,
            Type profile,
            Type input,
            Type data)
        {
            Context = context;
            Profile = profile;
            Input = input;
            Data = data;
        }

        public Type Context { get; }
        public Type Profile { get; }
        public Type Input { get; }
        public Type Data { get; }
    }
}
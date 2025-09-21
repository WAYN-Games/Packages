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

using Unity.Entities;

namespace WAYN.Locomotion.Demo.BasicLocomotion
{
    /// <summary>
    /// Component containing state data for basic locomotion mechanics.
    /// This struct is intended to be used within the locomotion state machine
    /// to track and manage state specific data, such as jumping count, timers, etc...
    /// </summary>
    public struct BasicLocomotionStateData : IComponentData
    {
        public JumpData JumpData;
    }
}
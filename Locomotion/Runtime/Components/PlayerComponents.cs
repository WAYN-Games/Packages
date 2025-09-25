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
using Locomotion.Runtime.Authoring.Player;
using Unity.Entities;
using UnityEngine;

namespace WAYNGames.Locomotion.Runtime.Components
{
    /// <summary>
    ///     List of character entity controlled by the player or AI Agent
    /// </summary>
    [Serializable]
    [InternalBufferCapacity(1)]
    public struct ControlledCharacters : IBufferElementData
    {
        public Entity Character;
        public Locomotor Locomotor;
        public Model Model;
    }

    /// <summary>
    ///     Set of references to mono objects.
    ///     These are used to update and managed game object behaviors of the player entity from systems allowing for greater
    ///     control on the player loop.
    /// </summary>
    [Serializable]
    public struct PlayerGameObjectInstance : ICleanupComponentData
    {
        /// <summary>
        ///     The camera in charge of rendering the POV of the player.
        /// </summary>
        public UnityObjectRef<GameObject> Instance;
    }

    /// <summary>
    ///     Set of references to mono objects.
    ///     These are used to update and managed game object behaviors of the player entity from systems allowing for greater
    ///     control on the player loop.
    /// </summary>
    [Serializable]
    public struct PlayerGameObjectPrefab : IComponentData
    {
        /// <summary>
        ///     The camera in charge of rendering the POV of the player.
        /// </summary>
        public UnityObjectRef<GameObject> Prefab;
    }

}
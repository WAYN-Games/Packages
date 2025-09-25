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
using UnityEngine;
using WAYNGames.Locomotion.Runtime.StateMachine;

public abstract class LocomotionProfileAsset<TProfile> : ScriptableObject
    where TProfile : unmanaged, ILocomotionProfile
{
    [SerializeField] protected CharacterPhysicsProfile PhysicsProfile;

    public abstract ref TProfile BuildBlobAsset(ref BlobBuilder blobBuilder);
}


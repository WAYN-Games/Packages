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

using Unity.Physics;

internal static class UnityPhysicsInternals
{
    /// <summary>
    /// Forces the provided collider to be non-unique by assigning a shared unique identifier.
    /// </summary>
    /// <param name="collider">The collider to modify, passed by reference.</param>
    internal static void ForceNonUniqueCollider(ref Collider collider)
    {
        collider.SetForceUniqueID(ColliderConstants.k_SharedBlobID);
    }
}
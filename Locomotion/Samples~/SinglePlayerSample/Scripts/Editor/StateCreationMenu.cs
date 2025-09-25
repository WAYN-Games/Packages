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
#if UNITY_EDITOR
using UnityEditor;
using WAYNGames.Locomotion.Editor;

namespace Locomotion.Samples.SinglePlayerSample.Scripts.Runtime.Editor
{

    public static class StateCreationMenu 
    {
        [MenuItem("Assets/Create/WAYN Games/Locomotion/BasicLocomotion/State")]
        public static void CreateAssemblyDefinition()
        {
            LocomotionStateCreationMenu.GenerateStateScript("BasicLocomotionContext", "BasicLocomotionProfile", "BasicLocomotionInput", "BasicLocomotionStateData");
        }
    }
}
#endif
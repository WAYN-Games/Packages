#if UNITY_EDITOR

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

using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace WAYNGames.Locomotion.Editor
{
    public static class LocomotionStateCreationMenu
    {

        const string StateTemplate = @"

using WAYNGames.Locomotion.Runtime.Components;
using WAYNGames.Locomotion.Runtime.StateMachine;

namespace #NAME_SPACE#
{
    internal struct
        #STATE_NAME# : ILocomotionState<#Context#, #Profile#, #Input#,#StateData#>
    { 
        /// <summary>
        /// Method invoked when entering a new locomotion state within the state machine.
        /// </summary>
        /// <param name=""ctx"">
        /// The context of the locomotion system, containing state, input, and profile data required for the state transition.
        /// </param>
        public static void OnEnterState(ref #Context# ctx)
        {
        }

        /// <summary>
        /// Method invoked to integrate the current state within the locomotion system, allowing for processing of inputs and computation of the displacement.
        /// </summary>
        /// <param name=""ctx"">
        /// The context of the locomotion system, containing state, input, and profile data necessary for the integration process.
        /// </param>
        public static void OnIntegrateState(ref #Context# ctx)
        {
        }

        /// <summary>
        /// Method to move the character based on the displacement computed during integration.
        /// This method represents one iteration of the ""Collide and Slide"" algorithm to resolve collision.
        /// After each move, the displacement should be updated to reflect the portion of displacement that could not be achieved due to a collision.
        /// This method will be re invoked, up to <see cref=""CharacterPhysicsProfile""/>.<c>MaxBounce</c> times, as long as the displacements in the context aren't set to 0.  
        /// </summary>
        /// <param name=""ctx"">
        /// The context of the locomotion system, containing state, input, and profile data relevant for handling collisions.
        /// </param>
        public static void OnResolveStateCollisions(ref #Context# ctx)
        {
        }

        /// <summary>
        /// Method invoked when exiting the current locomotion state within the state machine.
        /// </summary>
        /// <param name=""ctx"">
        /// The context of the locomotion system, containing state, input, and profile data relevant to the state being exited.
        /// </param>
        static void OnExitState(ref #Context# ctx)
        {
        }
    }
}";     


        public static void GenerateStateScript(string context, string profile, string input, string stateData)
        {
            string path = GetSelectedPathOrFallback();
            
            var nameSpace = FindRootNamespaceForFolder(path);


            string filePath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, "EmptyState.cs"));
            FileInfo f = new FileInfo(filePath);
            var stateName = f.Name.Replace(".cs","");
            File.WriteAllText(filePath, StateTemplate.Replace("#NAME_SPACE#", nameSpace)
                .Replace("#STATE_NAME#", stateName)
                .Replace("#Context#", context)
                .Replace("#Profile#", profile)
                .Replace("#Input#", input)
                .Replace("#StateData#", stateData));
            AssetDatabase.Refresh();
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(filePath);
            ProjectWindowUtil.ShowCreatedAsset(asset);
        }

        static string FindRootNamespaceForFolder(string path)
        {
            while (true)
            {
                var dir = new DirectoryInfo(path);

                var asmdefs = dir.GetFiles("*.asmdef", SearchOption.TopDirectoryOnly);
                switch (asmdefs.Length)
                {
                    case > 1:
                        Debug.LogError($"More than one assembly definition found in the folder {dir.FullName}. Please remove all but one.");
                        return ProjectGenerationRootNamespace();
                    case 1:
                        return ExtractRootNamespaceFromAsmdef(asmdefs[0].FullName);
                    default:
                    {
                        if (dir.Name != "Assets" && dir.Parent != null)
                        {
                            path = dir.Parent.FullName;
                            continue;
                        }
                        break;
                    }
                }

                return ProjectGenerationRootNamespace();
            }
        }

        static string ProjectGenerationRootNamespace()
        {
            return string.IsNullOrEmpty(EditorSettings.projectGenerationRootNamespace) ? "DefaultNamespace" : EditorSettings.projectGenerationRootNamespace;
        }

        static string ExtractRootNamespaceFromAsmdef(string asmdefPath)
        {
            FileInfo asmdefFile = new FileInfo(asmdefPath);
            string asmdef = File.ReadAllText(asmdefFile.FullName);
            JObject json = JObject.Parse(asmdef);
            return (string)json["rootNamespace"];;
        }

        private static string GetSelectedPathOrFallback()
        {
            string path = "Assets";
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                }
                break;
            }
            return path;
        }
    }
}
#endif
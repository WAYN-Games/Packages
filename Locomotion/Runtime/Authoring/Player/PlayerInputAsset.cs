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

/// <summary>
/// The <c>PlayerInputAsset</c> is an abstract base class for defining player input behavior
/// in a characters locomotion system. This class should be inherited to implement
/// specific player input handling, such as mapping game inputs to a given input type.
/// At runtime the asset is instantiated per player to allow caching of player specific reference (camera, etc...) in the asset instance.
/// </summary>
/// <remarks>
/// This asset reads the player input device but doesn't affect the controlled character input.
/// To apply the player input to a character you need to implement the <c>ICharacterLocomotionInput</c> on your <c>TInput</c>
/// </remarks>
/// <typeparam name="TInput">
/// The input type that implements the <see cref="ICharacterLocomotionInput{TInput}"/> interface,
/// representing how character locomotion inputs are structured and processed.
/// </typeparam>
public abstract class PlayerInputAsset<TInput> : ScriptableObject, IComponentData
    where TInput : unmanaged, ICharacterLocomotionInput<TInput>
{
    /// <summary>
    /// Reads input values and maps them to the specified input struct for the player character.
    /// </summary>
    /// <param name="inputs">The input struct to populate with input data.</param>
    public abstract void ReadInputsFromAsset(out TInput inputs);
    /// <summary>
    /// Performs the initialization of the input asset, enabling necessary input actions, caching camera or other necessary information to process the player input.
    /// </summary>
    public abstract void InitInputAsset(PlayerGameObjectInstance playerGameObjectInstance);

}

public interface ICharacterLocomotionInput<TInput> : IComponentData
{
    /// 
    /// Applies the provided input values to the character's locomotion system.
    /// This method allow for accumulation of input (such as triggers) to account for frame rate differences between the device capture (in update) and the input application to the character (in fixed update).
    /// <param name="playerInputs">The input structure containing movement and action data for the character.</param>
    public void ApplyInputsToCharacter(in TInput playerInputs);
}
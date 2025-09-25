using WAYNGames.Locomotion.Runtime.Components;
using Unity.Entities;

namespace Locomotion.Runtime.Authoring.Player
{
    /// <summary>
    /// A Unity ECS system that operates under the baking system filter.
    /// This system is part of the Locomotion.Runtime.Authoring.Player namespace
    /// and may contain logic related to the backing or processing of player character data during runtime.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial struct BasePlayerCharacterBackingSystem : ISystem
    {
        void OnUpdate(ref SystemState state)
        {
            var locomotorLookup = SystemAPI.GetComponentLookup<Locomotor>();
            var modelLookup = SystemAPI.GetComponentLookup<Model>();
            foreach (var (controlledCharactersList, controlledCharacters) in SystemAPI
                         .Query<DynamicBuffer<ControlledCharactersBakingList>, DynamicBuffer<ControlledCharacters>>())
            {
                controlledCharacters.Clear();
                foreach (ControlledCharactersBakingList controlledCharacter in controlledCharactersList)
                    controlledCharacters.Add(new ControlledCharacters
                    {
                        Character = controlledCharacter.Character,
                        Locomotor = locomotorLookup[controlledCharacter.Character],
                        Model = modelLookup[controlledCharacter.Character]
                    });
            }
        }
    }
}
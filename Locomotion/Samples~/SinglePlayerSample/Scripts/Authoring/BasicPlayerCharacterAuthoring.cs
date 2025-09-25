using Locomotion.Runtime.Authoring.Player;

namespace WAYN.Locomotion.Demo.BasicLocomotion
{
    public class BasicPlayerCharacterAuthoring : BasePlayerCharacterAuthoring<BasicLocomotionInput>
    {
        public class BasicPlayerCharacterBaker : BasePlayerCharacterBaker<BasicPlayerCharacterAuthoring>
        {
        }
    }
}
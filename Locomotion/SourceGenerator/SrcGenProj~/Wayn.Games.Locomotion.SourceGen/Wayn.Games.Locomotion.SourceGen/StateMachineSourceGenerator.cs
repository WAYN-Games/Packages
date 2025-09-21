using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TypeInfo = System.Reflection.TypeInfo;


namespace Wayn.Games.Locomotion.SourceGen;

/// <summary>
/// A sample source generator that creates a custom report based on class properties. The target class should be annotated with the 'Generators.ReportAttribute' attribute.
/// When using the source code as a baseline, an incremental source generator is preferable because it reduces the performance overhead.
/// </summary>
[Generator]
public class StateMachineSourceGenerator : IIncrementalGenerator
{

    const string Namespace = "Wayn.Locomotion.StateMachine";
    const string AttributeName = "LocomotionStateMachine";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        
        // Filter classes annotated with the [Report] attribute. Only filtered Syntax Nodes can trigger code generation.
        var stxProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                (s, _) => s is EnumDeclarationSyntax { AttributeLists.Count: > 0 },
                (ctx, _) => GetDeclarationForSourceGen(ctx))
            .Where(t => t.reportAttributeFound)
            .Select((t, _) => t.Item1);
        
        
        // Generate the source code.
        context.RegisterSourceOutput(stxProvider,GenerateCode);
        
    }


    /// <summary>
    /// Checks whether the Node is annotated with the [LocomotionStateMachine] attribute and maps syntax context to the specific node type (EnumDeclarationSyntax).
    /// </summary>
    /// <param name="context">Syntax context, based on CreateSyntaxProvider predicate</param>
    /// <returns>The specific cast and whether the attribute was found.</returns>
    private static (EnumDeclarationSyntax, bool reportAttributeFound) GetDeclarationForSourceGen(
        GeneratorSyntaxContext context)
    {
        var enumDeclarationSyntax = (EnumDeclarationSyntax)context.Node;

        // Go through all attributes of the class.
        foreach (AttributeListSyntax attributeListSyntax in enumDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                    continue; // if we can't get the symbol, ignore it

                string attributeName = attributeSymbol.ContainingType.ToDisplayString();

                // Check the full name of the [LocomotionStateMachine] attribute.
                if (attributeName == $"{Namespace}.{AttributeName}Attribute")
                    return (enumDeclarationSyntax, true);
            }
        }

        return (enumDeclarationSyntax, false);
    }

    /// <summary>
    /// Generate code action.
    /// It will be executed on specific nodes (EnumDeclarationSyntax annotated with the [LocomotionStateMachine] attribute) changed by the user.
    /// </summary>
    /// <param name="context">Source generation context used to add source files.</param>
    /// <param name="enumDeclarationSyntax">Nodes annotated with the [Report] attribute that trigger the generate action.</param>
    /// <param name="compilation"></param>
    private void GenerateCode(SourceProductionContext context,
        EnumDeclarationSyntax enumDeclarationSyntax)
    {
        
            var namespaceName = enumDeclarationSyntax.GetNameSpace();
            var usings = enumDeclarationSyntax.GetUsings();

            var smAttribute = enumDeclarationSyntax.GetAttribute(AttributeName);
            var contextName = smAttribute.GetAttributeArgument(0);
            var profileName = smAttribute.GetAttributeArgument(1);
            var inputName = smAttribute.GetAttributeArgument(2);
            var dataName = smAttribute.GetAttributeArgument(3);
            
            
            // 'Identifier' means the token of the node. Get class name from the syntax node.
            var enumName = enumDeclarationSyntax.Identifier.Text;


            GenerateStateMachine(context, enumDeclarationSyntax, enumName, usings, namespaceName, contextName, profileName, inputName, dataName);
            GenerateLocomotionSystem(context, enumDeclarationSyntax, enumName, usings, namespaceName, contextName, profileName, inputName, dataName);
            GenerateInputReaderSystem(context, enumDeclarationSyntax, enumName, usings, namespaceName, contextName, profileName, inputName, dataName);
            GeneratePlayerInputManagerSystem(context, enumDeclarationSyntax, enumName, usings, namespaceName, contextName, profileName, inputName, dataName);
    }

    static void GeneratePlayerInputManagerSystem(SourceProductionContext context, EnumDeclarationSyntax enumDeclarationSyntax,
        string enumName, string usings, string namespaceName, string contextName, string profileName, string inputName,
        string dataName)
    {
                // Build up the source code
        var code = $@"// <auto-generated/>
#pragma warning disable CS0105
{usings}
using Locomotion.Runtime.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Wayn.Locomotion.StateMachine;
#pragma warning restore CS0105

namespace {namespaceName}
{{
    /// <exclude />
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct {enumName}PlayerInputManagerSystem : ISystem
    {{
        EntityQuery m_UninitializedPlayerQuery;
        EntityQuery m_DestroyedPlayerQuery;
        
        public void OnCreate(ref SystemState state)
        {{
            m_UninitializedPlayerQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<LocomotionInput<{inputName}>>()
                .WithAll<PlayerGameObject>()
                .WithNone<LocomotionInputInitialized<{inputName}>>()
                .Build(ref state);
            
            m_DestroyedPlayerQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<LocomotionInputInitialized<{inputName}>>()
                .WithNone<LocomotionInput<{inputName}>>()
                .Build(ref state);
        }}
        
        public void OnUpdate(ref SystemState state)
        {{
            foreach (Entity entity in m_UninitializedPlayerQuery.ToEntityArray(Allocator.Temp))
            {{
                var inputAsset = state.EntityManager.GetComponentData<LocomotionInput<{inputName}>>(entity);
                var playerGameObject = state.EntityManager.GetComponentData<PlayerGameObject>(entity);
                var inputAssetInstance = Object.Instantiate(inputAsset.Asset.Value);
                inputAssetInstance.InitInputAsset(playerGameObject);
                state.EntityManager.AddComponentData(entity, new LocomotionInputInitialized<{inputName}>
                {{
                    Instance = inputAssetInstance
                }});
            }}

            foreach (Entity entity in m_DestroyedPlayerQuery.ToEntityArray(Allocator.Temp))
            {{
                var instance = state.EntityManager.GetComponentData<LocomotionInputInitialized<{inputName}>>(entity).Instance;
                Object.Destroy(instance);
                state.EntityManager.RemoveComponent<LocomotionInputInitialized<{inputName}>>(entity);
            }}
            
            
        }}
    }}
}}
";

        // Add the source code to the compilation.
        context.AddSource($"{enumName}PlayerInputManagerSystem.g.cs", SourceText.From(code, Encoding.UTF8));
        
    }

    static void GenerateInputReaderSystem(SourceProductionContext context, EnumDeclarationSyntax enumDeclarationSyntax,
        string enumName, string usings, string namespaceName, string contextName, string profileName, string inputName,
        string dataName)
    {
            
        // Build up the source code
        var code = $@"// <auto-generated/>
#pragma warning disable CS0105
{usings}
using Locomotion.Runtime.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Profiling;
using Wayn.Locomotion.StateMachine;
#pragma warning restore CS0105

namespace {namespaceName}
{{
    /// <exclude />
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
    public partial struct {enumName}InputReaderSystem : ISystem
    {{
        ComponentLookup<{inputName}> m_LocomotionInputLookup;
        BufferTypeHandle<ControlledCharacters> m_ControlledCharactersHandle;
        EntityTypeHandle m_PlayerEntityTypeHandle;
        EntityQuery m_PlayerQuery;

        NativeHashMap<Entity, {inputName}> m_PlayerInputs;

        public void OnCreate(ref SystemState state)
        {{
            m_LocomotionInputLookup = state.GetComponentLookup<{inputName}>();
            m_ControlledCharactersHandle = state.GetBufferTypeHandle<ControlledCharacters>(true);
            m_PlayerEntityTypeHandle = state.GetEntityTypeHandle();
            m_PlayerInputs = new NativeHashMap<Entity, {inputName}>(1, Allocator.Persistent);
            
            m_PlayerQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ControlledCharacters>()
                .WithAll<LocomotionInputInitialized<{inputName}>>()
                .Build(ref state);
        }}

        public void OnDestroy(ref SystemState state)
        {{
            m_PlayerInputs.Dispose();
        }}

        public void OnUpdate(ref SystemState state)
        {{
            m_PlayerInputs.Capacity = m_PlayerQuery.CalculateEntityCount();
            m_PlayerInputs.Clear();
            var players = m_PlayerQuery.ToEntityArray(Allocator.Temp);

            foreach (var player in players)
            {{
                var inputs = state.EntityManager.GetComponentData<LocomotionInputInitialized<{inputName}>>(player);
                inputs.Instance.Value.ReadInputsFromAsset(out {inputName} playerInputs);
                m_PlayerInputs.Add(player, playerInputs);
            }}
         
            
            m_LocomotionInputLookup.Update(ref state);
            m_ControlledCharactersHandle.Update(ref state);
            m_PlayerEntityTypeHandle.Update(ref state);
            state.Dependency = new ApplyInputsJob()
            {{
                PlayerInputs = m_PlayerInputs.AsReadOnly(),
                LocomotionInputLookup = m_LocomotionInputLookup,
                ControlledCharactersHandle = m_ControlledCharactersHandle,
                EntityTypeHandle = m_PlayerEntityTypeHandle
            }}.ScheduleParallel(m_PlayerQuery, state.Dependency);
            
        }}
        
        [BurstCompile]
        public struct ApplyInputsJob : IJobChunk
        {{
            
            [ReadOnly] public NativeHashMap<Entity, {inputName}>.ReadOnly PlayerInputs;
            [ReadOnly] public BufferTypeHandle<ControlledCharacters> ControlledCharactersHandle;
            [ReadOnly] public EntityTypeHandle EntityTypeHandle;
            [NativeDisableParallelForRestriction] public ComponentLookup<{inputName}> LocomotionInputLookup;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {{
                var controlledCharactersAccessor = chunk.GetBufferAccessor(ref ControlledCharactersHandle);
                var entities = chunk.GetNativeArray(EntityTypeHandle);

                for (int i = 0; i < entities.Length; i++)
                {{
                    var entity = entities[i];
                    var controlledCharacters = controlledCharactersAccessor[i];
                    foreach (var controlledCharacter in controlledCharacters)
                    {{
                        {inputName} input = LocomotionInputLookup[controlledCharacter.Character.Locomotion];
                        input.ApplyInputsToCharacter(PlayerInputs[entity]);
                        LocomotionInputLookup[controlledCharacter.Character.Locomotion] = input;
                    }}
                }}
            }}
        }}
    }}
}}
";

        // Add the source code to the compilation.
        context.AddSource($"{enumName}InputReaderSystem.g.cs", SourceText.From(code, Encoding.UTF8));
    }
    
     static void GenerateLocomotionSystem(SourceProductionContext context, EnumDeclarationSyntax enumDeclarationSyntax,
        string enumName, string usings, string namespaceName, string contextName, string profileName, string inputName,
        string dataName)
    {
            
        // Build up the source code
        var code = $@"// <auto-generated/>
#pragma warning disable CS0105
{usings}
using Locomotion.Runtime.Components;
using Locomotion.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Wayn.Locomotion.StateMachine;
#pragma warning restore CS0105

namespace {namespaceName}
{{
    /// <exclude />
    [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    [UpdateBefore(typeof(LocomotionDrivenSyncSystem))]
    [UpdateAfter(typeof(MotionTrackingSystem))]
    [BurstCompile]
    public partial struct {enumName}LocomotionSystem : ISystem
    {{
        EntityTypeHandle _entityHandle;
        ComponentTypeHandle<PhysicsCollider> _physicsColliderHandle;

        ComponentTypeHandle<LocomotionGravity> _locomotionGravityHandle;

        ComponentTypeHandle<LocomotionVelocity> _locomotionVelocityHandle;
        ComponentTypeHandle<LocomotionContact> _locomotionContactHandle;

        ComponentTypeHandle<LocalTransform> _transformHandle;
        ComponentTypeHandle<{inputName}> _locomotionInputHandle;
        ComponentTypeHandle<{dataName}> _locomotionStateDataHandle;
        
        ComponentLookup<TrackedMotion> _platformMotionFrameLookup;
        ComponentTypeHandle<LocomotionProfileBlob<{profileName}>> _locomotionProfileReferenceHandle;
        public ComponentTypeHandle<{enumName}StateMachine> _locomotionStateMachineHandle;


        EntityQuery _query;
        EntityQuery _physicsSingletonQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {{
            state.RequireForUpdate<PhysicsWorldSingleton>();
 
            _physicsSingletonQuery = state.GetEntityQuery(ComponentType.ReadOnly<PhysicsWorldSingleton>());

            _query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<LocalTransform>()
                .WithAll<LocomotionVelocity>()
                .WithAll<LocomotionContact>()
                .WithAll<LocomotionGravity>()
                .WithAll<PhysicsCollider>()
                .WithAll<{enumName}StateMachine>()
                .WithAll<{inputName}>()
                .WithAll<{dataName}>()
                .WithAll<LocomotionProfileBlob<{profileName}>>()
                .Build(ref state);

            _entityHandle = state.GetEntityTypeHandle();
            _transformHandle = state.GetComponentTypeHandle<LocalTransform>();

            _locomotionVelocityHandle = state.GetComponentTypeHandle<LocomotionVelocity>();
            _locomotionContactHandle = state.GetComponentTypeHandle<LocomotionContact>();

            _locomotionStateMachineHandle = state.GetComponentTypeHandle<{enumName}StateMachine>();
            _locomotionInputHandle = state.GetComponentTypeHandle<{inputName}>();
            _locomotionStateDataHandle = state.GetComponentTypeHandle<{dataName}>();
            _locomotionGravityHandle = state.GetComponentTypeHandle<LocomotionGravity>();
            _locomotionProfileReferenceHandle =
                state.GetComponentTypeHandle<LocomotionProfileBlob<{profileName}>>();
            _physicsColliderHandle = state.GetComponentTypeHandle<PhysicsCollider>(true);
            _platformMotionFrameLookup = state.GetComponentLookup<TrackedMotion>(true);
        }}
  
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {{
            _entityHandle.Update(ref state);
            _transformHandle.Update(ref state);

            _locomotionVelocityHandle.Update(ref state);
            _locomotionInputHandle.Update(ref state);
            _locomotionStateDataHandle.Update(ref state);
            _locomotionGravityHandle.Update(ref state);

            _physicsColliderHandle.Update(ref state);
            _platformMotionFrameLookup.Update(ref state);
            _locomotionProfileReferenceHandle.Update(ref state);
            _locomotionContactHandle.Update(ref state);
            _locomotionStateMachineHandle.Update(ref state);

            state.Dependency =
                new LocomotionJob<{enumName}StateMachine, {enumName}, {contextName},
                    {profileName}, {inputName},{dataName}>
                {{
                    PhysicsWorld = _physicsSingletonQuery.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
                    PlatformMotionFrameLookup = _platformMotionFrameLookup,
                    DeltaTime = state.WorldUnmanaged.Time.DeltaTime,
                    Entity = _entityHandle,
                    PhysicsCollider = _physicsColliderHandle,
                    LocomotionGravity = _locomotionGravityHandle,
                    LocomotionProfile = _locomotionProfileReferenceHandle,
                    LocomotionStateMachine = _locomotionStateMachineHandle,
                    Transform = _transformHandle,
                    LocomotionInput = _locomotionInputHandle,
                    LocomotionStateData = _locomotionStateDataHandle,
                    LocomotionVelocity = _locomotionVelocityHandle,
                    LocomotionContact = _locomotionContactHandle
                }}.ScheduleParallel(_query, state.Dependency);
        }}
    }}
}}
";

        // Add the source code to the compilation.
        context.AddSource($"{enumName}MovementSystem.g.cs", SourceText.From(code, Encoding.UTF8));
    }

    
    static void GenerateStateMachine(SourceProductionContext context, EnumDeclarationSyntax enumDeclarationSyntax,
        string enumName, string usings, string namespaceName, string contextName, string profileName, string inputName,
        string dataName)
    {
        List<string> switchOnResolveStateCollisions = new List<string>();
        List<string> switchOnOnIntegrateState = new List<string>();
        List<string> switchEvaluateTransitions = new List<string>();
            
        foreach (var member in enumDeclarationSyntax.Members)
        {
            var attribute = member.GetAttribute("LocomotionState");
            var stateName = member.Identifier.Text;
            var stateType = attribute.GetAttributeArgument(0);
                
            var caseOnResolveStateCollisions = $@"
                        case {enumName}.{stateName}:
                            {stateType}.OnResolveStateCollisions(ref context);
                            break;
";
            switchOnResolveStateCollisions.Add(caseOnResolveStateCollisions);
                
            var caseOnIntegrateState = $@"
                        case {enumName}.{stateName}:
                            {stateType}.OnIntegrateState(ref context);
                            break;
";
            switchOnOnIntegrateState.Add(caseOnIntegrateState);
            var caseEvaluateTransitions = $@"
                        case {enumName}.{stateName}:
                            {stateType}.EvaluateTransitions(ref context, out newState);
                            break;
";
            switchEvaluateTransitions.Add(caseEvaluateTransitions);
                
        }
            
            
        // Build up the source code
        var code = $@"// <auto-generated/>
#pragma warning disable CS0105
{usings}
using {namespaceName};
using Wayn.Locomotion.StateMachine;
using Unity.Entities;
#pragma warning restore CS0105

[assembly: RegisterGenericComponentType(typeof(LocomotionInput<{inputName}>))]
[assembly: RegisterGenericComponentType(typeof(LocomotionInputInitialized<{inputName}>))]
[assembly: RegisterGenericComponentType(typeof(LocomotionProfileBlob<{profileName}>))]

namespace {namespaceName}
{{
    /// <exclude />
    public partial struct {enumName}StateMachine : IComponentData,ILocomotionStateMachine<{enumName}StateMachine, {namespaceName}.{enumName}, 
    {contextName}, {profileName}, {inputName}, {dataName}> 
    {{
        // field for ref access and editor display
        public {namespaceName}.{enumName} currentState;

        // Explicit property forced by interface
        public {namespaceName}.{enumName} CurrentState
        {{
            get => currentState;
            set => currentState = value;
        }}

        public void ResolveCollision(ref {contextName} context)
        {{
            switch (CurrentState)
            {{
                {string.Join("\n", switchOnResolveStateCollisions)}
            }}
        }}
        public void Integrate(ref {contextName} context)
        {{
            switch (CurrentState)
            {{
                {string.Join("\n", switchOnOnIntegrateState)}
            }}
        }}
        public void EvaluateTransition(ref {contextName} context)
        {{
            BasicLocomotionStates newState = CurrentState;
            switch (CurrentState)
                        {{
                {string.Join("\n", switchEvaluateTransitions)}
            }}
            CurrentState = newState;
        }}
    }}
}}
";

        // Add the source code to the compilation.
        context.AddSource($"{enumName}.g.cs", SourceText.From(code, Encoding.UTF8));
    }
}

static class SyntaxExtensions
{


    public static AttributeSyntax GetAttribute(this MemberDeclarationSyntax declarationSyntax, string attributeName)
    {
        return declarationSyntax.AttributeLists
            .SelectMany(list => list.Attributes)
            .FirstOrDefault(attr => attr.Name.ToString() == attributeName);
    }
    
    /// <summary>
    /// Gets the string form of an argument from an attribute (by position or name).
    /// If a SemanticModel is provided, will try to resolve typeof(...) to a fully qualified name.
    /// </summary>
    public static string GetAttributeArgument(
        this AttributeSyntax attribute,
        int position = 0,
        SemanticModel semanticModel = null)
    {
        if (attribute.ArgumentList == null || attribute.ArgumentList.Arguments.Count == 0)
            return null;

        // Pick argument by name or position
        AttributeArgumentSyntax arg = null;

        if (position < attribute.ArgumentList.Arguments.Count)
        {
            arg = attribute.ArgumentList.Arguments[position];
        }

        if (arg == null) return null;

        // Handle typeof(...)
        if (arg.Expression is TypeOfExpressionSyntax tof)
        {
            if (semanticModel != null)
            {
                var symbol = semanticModel.GetTypeInfo(tof.Type).Type;
                return symbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::","");
            }
            return tof.Type.ToString();
        }

        // Fallback: return raw text
        return arg.Expression.ToString();
    }
    
    public static string GetUsings(this BaseTypeDeclarationSyntax declarationSyntax)
    {
        var root = declarationSyntax.SyntaxTree.GetRoot();
        var usings = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();
        var usingsString = string.Join("\n", usings.Select(u => u.ToString()));
        return usingsString;
    }

    public static string GetNameSpace(this BaseTypeDeclarationSyntax declarationSyntax)
    {
        var ancestorCount = 0;
        var parent = declarationSyntax.Parent;
        while (parent is BaseNamespaceDeclarationSyntax or BaseTypeDeclarationSyntax)
        {
            ancestorCount++;
            parent = parent.Parent;
        }
        parent = declarationSyntax.Parent;
        
        var names = new string[ancestorCount];
        var currentAncestor = ancestorCount - 1;
        while (parent is BaseNamespaceDeclarationSyntax or BaseTypeDeclarationSyntax)
        {
            switch (parent)
            {
                case BaseTypeDeclarationSyntax parentClass:
                    names[currentAncestor] = parentClass.Identifier.Text;
                    break;
                case BaseNamespaceDeclarationSyntax parentNamespace:
                    names[currentAncestor] = parentNamespace.Name.ToString();
                    break;
            }

            currentAncestor--;
            parent = parent.Parent;
        }

        StringBuilder namespaceBuilder = new StringBuilder();
        namespaceBuilder.Append(string.Join(".",names));
        return namespaceBuilder.ToString();
    }
}

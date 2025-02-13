using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.CompilationPipeline.Common.Diagnostics;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace PurrNet.Codegen
{
    public static class MakeSureOverrideIsCalled
    {
        public static void Process(MethodDefinition method, [UsedImplicitly] List<DiagnosticMessage> messages)
        {
            var networkIdentityType = method.Module.GetTypeDefinition<NetworkIdentity>();
            if (!IsCallingBase(method, method.Name))
            {
                var body = method.Body.GetILProcessor();
                var firstInstruction = method.Body.Instructions[0];
                var baseMethod = networkIdentityType.GetMethod(method.Name).Import(method.Module);
                
                body.InsertBefore(firstInstruction, body.Create(OpCodes.Ldarg_0));
                body.InsertBefore(firstInstruction, body.Create(OpCodes.Call, baseMethod));
            }
        }
        
        static bool IsCallingBase(MethodDefinition method, string methodName)
        {
            var instructions = method.Body.Instructions;
            for (int i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];
                if (instruction.OpCode.Code == Code.Call && instruction.Operand is MethodReference methodReference)
                {
                    if (methodReference.Name == methodName && PostProcessor.InheritsFrom(methodReference.DeclaringType.Resolve(), typeof(NetworkIdentity).FullName))
                        return true;
                }
            }
            return false;
        }

        public static bool ShouldProcess(MethodDefinition method)
        {
            if (method.DeclaringType.FullName == typeof(NetworkIdentity).FullName)
                return false;
            
            if (method.ReturnType != method.Module.TypeSystem.Void)
                return false;
            
            if (method.Parameters.Count > 0)
                return false;
            
            if (method.IsStatic)
                return false;
            
            if (method.HasGenericParameters)
                return false;

            return method.Name switch
            {
                "OnDestroy" => true,
                _ => false
            };
        }
    }
}

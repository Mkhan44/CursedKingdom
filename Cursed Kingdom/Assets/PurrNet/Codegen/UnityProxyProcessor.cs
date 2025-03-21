using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace PurrNet.Codegen
{
    public static class UnityProxyProcessor
    {
        public static void Process(TypeDefinition type, [UsedImplicitly] List<DiagnosticMessage> messages)
        {
            try
            {
                bool isProxyItself = type.FullName == typeof(UnityProxy).FullName;

                if (isProxyItself)
                    return;

                var module = type.Module;

                string objectClassFullName = typeof(UnityEngine.Object).FullName;
                var unityProxyType = module.GetTypeReference(typeof(UnityProxy)).Import(module).Resolve();

                foreach (var method in type.Methods)
                {
                    if (method.Body == null) continue;

                    var processor = method.Body.GetILProcessor();

                    for (var i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        var instruction = method.Body.Instructions[i];

                        if (instruction.Operand is not MethodReference methodReference)
                            continue;

                        if (methodReference.DeclaringType.FullName != objectClassFullName)
                            continue;

                        if (methodReference.Name != "Instantiate" && methodReference.Name != "Destroy")
                            continue;

                        var resolved = methodReference.Resolve();

                        if (resolved == null)
                            continue;

                        var targetMethod = GetInstantiateDefinition(resolved, unityProxyType);

                        if (targetMethod == null)
                            continue;

                        var targerRef = module.ImportReference(targetMethod);

                        if (methodReference is GenericInstanceMethod genericInstanceMethod)
                        {
                            var genRef = new GenericInstanceMethod(targerRef);

                            for (var j = 0; j < genericInstanceMethod.GenericArguments.Count; j++)
                                genRef.GenericArguments.Add(genericInstanceMethod.GenericArguments[j]);

                            for (var j = 0; j < genRef.GenericParameters.Count; j++)
                                genRef.GenericParameters.Add(genRef.GenericParameters[j]);

                            targerRef = module.ImportReference(genRef);
                        }

                        processor.Replace(instruction, processor.Create(OpCodes.Call, targerRef));
                    }
                }
            }
            catch (Exception e)
            {
                messages.Add(new DiagnosticMessage
                {
                    MessageData = $"Failed to process UnityProxy: {e.Message}",
                    DiagnosticType = DiagnosticType.Error
                });
            }
        }

        static MethodDefinition GetInstantiateDefinition(
            MethodReference originalMethod,
            TypeDefinition declaringType)
        {
            foreach (var method in declaringType.Methods)
            {
                if (method.Name != originalMethod.Name)
                    continue;

                if (method.Parameters.Count != originalMethod.Parameters.Count)
                    continue;

                // Check for matching generic parameters
                if (method.HasGenericParameters != originalMethod.HasGenericParameters)
                    continue;

                if (method.HasGenericParameters)
                {
                    if (method.GenericParameters.Count != originalMethod.GenericParameters.Count)
                        continue;

                    for (int i = 0; i < method.GenericParameters.Count; i++)
                    {
                        var originalParam = originalMethod.GenericParameters[i];
                        var candidateParam = method.GenericParameters[i];

                        // Compare names and constraints
                        if (originalParam.Name != candidateParam.Name)
                            goto NextMethod;

                        if (originalParam.Constraints.Count != candidateParam.Constraints.Count)
                            goto NextMethod;

                        for (int j = 0; j < originalParam.Constraints.Count; j++)
                        {
                            if (!TypesMatch(originalParam.Constraints[j].ConstraintType,
                                    candidateParam.Constraints[j].ConstraintType))
                                goto NextMethod;
                        }
                    }
                }

                // Check parameters
                bool match = true;
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    var originalParamType = originalMethod.Parameters[i].ParameterType;
                    var candidateParamType = method.Parameters[i].ParameterType;

                    if (!TypesMatch(originalParamType, candidateParamType))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return method;

                NextMethod:
                // ReSharper disable once RedundantJumpStatement
                continue;
            }

            return null;
        }

        static bool TypesMatch(TypeReference original, TypeReference candidate)
        {
            if (original.FullName != candidate.FullName)
                return false;

            // If either type is generic, check their arguments
            if (original is GenericInstanceType originalGeneric && candidate is GenericInstanceType candidateGeneric)
            {
                if (originalGeneric.GenericArguments.Count != candidateGeneric.GenericArguments.Count)
                    return false;

                for (int i = 0; i < originalGeneric.GenericArguments.Count; i++)
                {
                    if (!TypesMatch(originalGeneric.GenericArguments[i], candidateGeneric.GenericArguments[i]))
                        return false;
                }
            }

            return true;
        }
    }
}
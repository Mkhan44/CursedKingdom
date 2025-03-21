using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PurrNet.Packing;

namespace PurrNet.Codegen
{
    public static class GenerateDeltaSerializersProcessor
    {
        public static void HandleType(AssemblyDefinition assembly, TypeReference type, TypeDefinition generatedClass)
        {
            var bitStreamType = assembly.MainModule.GetTypeDefinition(typeof(BitPacker)).Import(assembly.MainModule);
            var packerType = assembly.MainModule.GetTypeDefinition(typeof(Packer)).Import(assembly.MainModule);

            var writeMethod = new MethodDefinition("WriteDelta", MethodAttributes.Public | MethodAttributes.Static,
                assembly.MainModule.TypeSystem.Boolean);
            var readMethod = new MethodDefinition("ReadDelta", MethodAttributes.Public | MethodAttributes.Static,
                assembly.MainModule.TypeSystem.Void);

            CreateWriteMethod(assembly.MainModule, writeMethod, type, bitStreamType, packerType);
            CreateReadMethod(assembly.MainModule, readMethod, type, bitStreamType);

            generatedClass.Methods.Add(writeMethod);
            generatedClass.Methods.Add(readMethod);
        }

        private static void CreateReadMethod(ModuleDefinition module, MethodDefinition method, TypeReference typeRef,
            TypeReference bitStreamType)
        {
            var packerGenType = module.GetTypeDefinition(typeof(Packer<>)).Import(module);
            var deltaPackerGenType = module.GetTypeDefinition(typeof(DeltaPacker<>)).Import(module);

            var serializer = packerGenType.GetMethod("Read").Import(module);
            var deltaSerializer = deltaPackerGenType.GetMethod("Read").Import(module);
            var packerTypeBoolean =
                GenerateSerializersProcessor.CreateGenericMethod(packerGenType, module.TypeSystem.Boolean, serializer,
                    module);

            var streamArg = new ParameterDefinition("stream", ParameterAttributes.None, bitStreamType);
            var oldValueArg = new ParameterDefinition("oldValue", ParameterAttributes.None, typeRef);
            var valueArg = new ParameterDefinition("value", ParameterAttributes.None, new ByReferenceType(typeRef));

            var type = typeRef.Resolve();
            var isEqualVar = new VariableDefinition(module.TypeSystem.Boolean);
            bool isClass = !type.IsValueType;

            method.Parameters.Add(streamArg);
            method.Parameters.Add(oldValueArg);
            method.Parameters.Add(valueArg);
            method.Body = new MethodBody(method)
            {
                InitLocals = true
            };

            method.Body.Variables.Add(isEqualVar);

            var il = method.Body.GetILProcessor();
            var endOfFunction = il.Create(OpCodes.Ret);
            var elseBlock = il.Create(OpCodes.Ldarg_2);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloca_S, isEqualVar);
            il.Emit(OpCodes.Call, packerTypeBoolean);
            il.Emit(OpCodes.Ldloc_0);

            // if true, return
            il.Emit(OpCodes.Brfalse, elseBlock);

            if (isClass)
            {
                var readIsNull = bitStreamType.GetMethod("ReadIsNull", true).Import(module);
                var genericIsNull = new GenericInstanceMethod(readIsNull);
                genericIsNull.GenericArguments.Add(typeRef);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Call, genericIsNull);
                il.Emit(OpCodes.Brfalse, endOfFunction);
            }

            if (type.IsEnum)
            {
                return;
            }

            foreach (var field in type.Fields)
            {
                if (field.IsStatic)
                    continue;

                bool isDelegate = PostProcessor.InheritsFrom(field.FieldType.Resolve(), typeof(Delegate).FullName);

                if (isDelegate)
                    continue;

                var fieldType = GenerateSerializersProcessor.ResolveGenericFieldType(field, typeRef);

                if (fieldType == null)
                    continue;

                var packer =
                    GenerateSerializersProcessor.CreateGenericMethod(deltaPackerGenType, fieldType, deltaSerializer,
                        module);

                if (!field.IsPublic)
                {
                    var variable = new VariableDefinition(fieldType);
                    method.Body.Variables.Add(variable);

                    var getter = GenerateSerializersProcessor.MakeFullNameValidCSharp($"Purrnet_Get_{field.Name}");
                    var setter = GenerateSerializersProcessor.MakeFullNameValidCSharp($"Purrnet_Set_{field.Name}");

                    var getterReference = new MethodReference(getter, fieldType, typeRef)
                    {
                        HasThis = true
                    };

                    var setterReference = new MethodReference(setter, module.TypeSystem.Void, typeRef)
                    {
                        HasThis = true
                    };

                    setterReference.Parameters.Add(
                        new ParameterDefinition("value", ParameterAttributes.None, fieldType));

                    il.Emit(OpCodes.Ldarg_0);

                    if (isClass)
                        il.Emit(OpCodes.Ldarg_1);
                    else il.Emit(OpCodes.Ldarga_S, oldValueArg);

                    il.Emit(OpCodes.Call, getterReference);

                    il.Emit(OpCodes.Ldloca, variable);
                    il.Emit(OpCodes.Call, packer);

                    il.Emit(OpCodes.Ldarg_2);
                    if (isClass) il.Emit(OpCodes.Ldind_Ref);
                    il.Emit(OpCodes.Ldloc, variable);
                    il.Emit(OpCodes.Call, setterReference);

                    continue;
                }

                var fieldRef = new FieldReference(field.Name, field.FieldType, typeRef).Import(module);
                il.Emit(OpCodes.Ldarg_0);

                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldfld, fieldRef);

                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldflda, fieldRef);

                il.Emit(OpCodes.Call, packer);
            }

            il.Emit(OpCodes.Br, endOfFunction);
            il.Append(elseBlock);

            // value = oldValue

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stobj, typeRef);

            il.Append(endOfFunction);
        }

        private static void CreateWriteMethod(ModuleDefinition module, MethodDefinition method, TypeReference typeRef,
            TypeReference bitStreamType, TypeReference packerType)
        {
            var bitPackerType = module.GetTypeDefinition(typeof(BitPacker)).Import(module);
            var deltaPackerGenType = module.GetTypeDefinition(typeof(DeltaPacker<>)).Import(module);

            var deltaSerializer = deltaPackerGenType.GetMethod("Write").Import(module);
            var advanceBits = bitPackerType.GetMethod("AdvanceBits", false).Import(module);
            var writeAt = bitPackerType.GetMethod("WriteAt", false).Import(module);
            var setBitPosition = bitPackerType.GetMethod("SetBitPosition", false).Import(module);

            var streamArg = new ParameterDefinition("stream", ParameterAttributes.None, bitStreamType);
            var oldValueArg = new ParameterDefinition("oldValue", ParameterAttributes.None, typeRef);
            var valueArg = new ParameterDefinition("value", ParameterAttributes.None, typeRef);

            var type = typeRef.Resolve();
            var flagPos = new VariableDefinition(module.TypeSystem.Int32);
            var isEqualVar = new VariableDefinition(module.TypeSystem.Boolean);

            bool isClass = !type.IsValueType;

            method.Parameters.Add(streamArg);
            method.Parameters.Add(oldValueArg);
            method.Parameters.Add(valueArg);
            method.Body = new MethodBody(method)
            {
                InitLocals = true
            };

            method.Body.Variables.Add(flagPos);
            method.Body.Variables.Add(isEqualVar);

            var il = method.Body.GetILProcessor();
            var endOfFunction = il.Create(OpCodes.Nop);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Call, advanceBits);
            il.Emit(OpCodes.Stloc_0);

            if (isClass)
            {
                var writeIsNull = bitStreamType.GetMethod("HandleNullScenarios", true).Import(module);
                var genericIsNull = new GenericInstanceMethod(writeIsNull);
                genericIsNull.GenericArguments.Add(typeRef);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldloca_S, isEqualVar);
                il.Emit(OpCodes.Call, genericIsNull);

                // if null return
                il.Emit(OpCodes.Brfalse, endOfFunction);
            }

            if (type.IsEnum)
            {
                return;
            }

            for (var i = 0; i < type.Fields.Count; i++)
            {
                var field = type.Fields[i];
                if (field.IsStatic)
                    continue;

                bool isDelegate = PostProcessor.InheritsFrom(field.FieldType.Resolve(), typeof(Delegate).FullName);

                if (isDelegate)
                    continue;

                var fieldType = GenerateSerializersProcessor.ResolveGenericFieldType(field, typeRef);

                if (fieldType == null)
                    continue;

                var packer =
                    GenerateSerializersProcessor.CreateGenericMethod(deltaPackerGenType, fieldType, deltaSerializer,
                        module);

                if (!field.IsPublic)
                {
                    var variable = new VariableDefinition(fieldType);
                    method.Body.Variables.Add(variable);

                    var getter = GenerateSerializersProcessor.MakeFullNameValidCSharp($"Purrnet_Get_{field.Name}");
                    var getterReference = new MethodReference(getter, fieldType, typeRef)
                    {
                        HasThis = true
                    };

                    il.Emit(OpCodes.Ldarg_0);

                    if (isClass)
                        il.Emit(OpCodes.Ldarg_1);
                    else il.Emit(OpCodes.Ldarga_S, oldValueArg);

                    il.Emit(OpCodes.Call, getterReference);

                    if (isClass)
                        il.Emit(OpCodes.Ldarg_2);
                    else il.Emit(OpCodes.Ldarga_S, valueArg);

                    il.Emit(OpCodes.Call, getterReference);
                }
                else
                {
                    var fieldRef = new FieldReference(field.Name, field.FieldType, typeRef).Import(module);

                    il.Emit(OpCodes.Ldarg_0);

                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldfld, fieldRef);

                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Ldfld, fieldRef);
                }

                il.Emit(OpCodes.Call, packer);

                if (i > 0)
                {
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Or);
                }

                il.Emit(OpCodes.Stloc_1);
            }

            // WriteAt
            il.Append(endOfFunction);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Call, writeAt);

            // if (isEqual)
            var endOfIf = il.Create(OpCodes.Nop);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Brtrue, endOfIf);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Call, setBitPosition);

            il.Append(endOfIf);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ret);

            method.DebugInformation.Scope =
                new ScopeDebugInformation(method.Body.Instructions[0], method.Body.Instructions[^1]);
            method.DebugInformation.Scope.Variables.Add(new VariableDebugInformation(flagPos, "flagPos"));
            method.DebugInformation.Scope.Variables.Add(new VariableDebugInformation(isEqualVar, "wasChanged"));
        }

        public static void HandleGenericType(AssemblyDefinition assembly, TypeReference type,
            HandledGenericTypes genericT)
        {
            // TODO: Implement
        }
    }
}
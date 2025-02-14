using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PurrNet.Modules;
using PurrNet.Packing;
using PurrNet.Pooling;
using Unity.CompilationPipeline.Common.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PurrNet.Codegen
{
    public enum HandledGenericTypes
    {
        None,
        List,
        Array,
        HashSet,
        Dictionary,
        Nullable,
        Queue,
        Stack,
        DisposableList,
        DisposableHashSet
    }
    
    public static class GenerateSerializersProcessor
    {
        public static bool ValideType(TypeReference type)
        {
            // Check if the type itself is an interface
            if (type.Resolve()?.IsInterface == true)
            {
                return false;
            }
            
            bool isDelegate = PostProcessor.InheritsFrom(type.Resolve(), typeof(Delegate).FullName);

            if (isDelegate)
                return false;

            // Check if the type is a generic instance
            if (type is GenericInstanceType genericInstance)
            {
                if (genericInstance.GenericArguments.Count == 0)
                    return false;
                
                // Recursively validate all generic arguments
                foreach (var argument in genericInstance.GenericArguments)
                {
                    if (argument.ContainsGenericParameter || argument.Resolve()?.IsInterface == true || !ValideType(argument))
                    {
                        return false;
                    }
                }
            }
            else if (type.ContainsGenericParameter)
            {
                // If the type itself contains generic parameters (e.g., T)
                return false;
            }

            if (type.HasGenericParameters)
            {
                foreach (var param in type.GenericParameters)
                {
                    if (!PostProcessor.IsConcreteType(param, out _))
                        return false;
                }
            }

            // If no open generics or interfaces are found, return true
            return true;
        }
        
        public static string MakeFullNameValidCSharp(string name)
        {
            return name.Replace("<", "_").Replace(">", "_").Replace(",", "_").Replace(" ", "_").Replace(".", "_").Replace("`", "_").Replace("/", "_").Replace("[", "_I_").Replace("]", "_I_");
        }
        
        public static void HandleType(bool hashOnly, AssemblyDefinition assembly, TypeReference type, HashSet<string> visited, bool isEditor, List<DiagnosticMessage> messages)
        {
            if (!visited.Add(type.FullName))
                return;

            if (!ValideType(type))
                return;
            
            if (!PostProcessor.IsTypeInOwnModule(type, assembly.MainModule))
                return;
            
            string namespaceName = type.Namespace;
            if (string.IsNullOrWhiteSpace(namespaceName))
                namespaceName = "PurrNet.CodeGen.Serializers";
            else namespaceName += ".PurrNet.CodeGen.Serializers";
            
            // create static class
            var serializerClass = new TypeDefinition(namespaceName, 
                $"{MakeFullNameValidCSharp(type.FullName)}_Serializer",
                TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Abstract | TypeAttributes.Public,
                assembly.MainModule.TypeSystem.Object
            );
            
            var resolvedType = type.Resolve();
            
            if (resolvedType == null)
                return;
            
            if (resolvedType.IsInterface)
                return;
            
            bool isNetworkIdentity = PostProcessor.InheritsFrom(resolvedType, typeof(NetworkIdentity).FullName);
            bool isNetworkModule = PostProcessor.InheritsFrom(resolvedType, typeof(NetworkModule).FullName);
            bool hasINetworkModule = HasInterface(resolvedType, typeof(INetworkModule));
            
            if (!isNetworkIdentity && !isNetworkModule && PostProcessor.InheritsFrom(resolvedType, typeof(Object).FullName) && 
                !HasInterface(resolvedType, typeof(IPacked)) && 
                !HasInterface(resolvedType, typeof(IPackedAuto)) && 
                !HasInterface(resolvedType, typeof(IPackedSimple)))
                return;

            if (hasINetworkModule)
                return;
            
            var bitStreamType = assembly.MainModule.GetTypeDefinition(typeof(BitPacker)).Import(assembly.MainModule);
            var mainmodule = assembly.MainModule;
            
            assembly.MainModule.Types.Add(serializerClass);

            if (hashOnly)
            {
                HandleHashOnly(assembly, type, serializerClass, isEditor);
                return;
            }

            if (IsGeneric(type, out var genericT))
            {
                GenerateDeltaSerializersProcessor.HandleGenericType(assembly, type, genericT, messages);
                HandleGenerics(assembly, type, genericT, serializerClass, isEditor);
                return;
            }
            
            if (isNetworkIdentity)
            {
                HandleNetworkIdentity(assembly, type, serializerClass, isEditor);
                return;
            }

            if (isNetworkModule)
            {
                HandleNetworkModule(assembly, type, serializerClass, isEditor);
                return;
            }
            
            // create static write method
            var writeMethod = new MethodDefinition("Write", MethodAttributes.Public | MethodAttributes.Static, assembly.MainModule.TypeSystem.Void);
            var valueArg = new ParameterDefinition("value", ParameterAttributes.None, type);
            var streamArg = new ParameterDefinition("stream", ParameterAttributes.None, bitStreamType);
            writeMethod.Parameters.Add(streamArg);
            writeMethod.Parameters.Add(valueArg);
            writeMethod.Body = new MethodBody(writeMethod)
            {
                InitLocals = true
            };
            
            var packerType = mainmodule.GetTypeDefinition(typeof(Packer<>)).Import(mainmodule);
            var readMethodP = packerType.GetMethod("Read").Import(mainmodule);
            var writeMethodP = packerType.GetMethod("Write").Import(mainmodule);
            
            var write = writeMethod.Body.GetILProcessor();
            GenerateMethod(true, writeMethod, writeMethodP, type, write, mainmodule, valueArg);
            serializerClass.Methods.Add(writeMethod);
            
            // create static read method
            var readMethod = new MethodDefinition("Read", MethodAttributes.Public | MethodAttributes.Static, assembly.MainModule.TypeSystem.Void);
            readMethod.Parameters.Add(new ParameterDefinition("stream", ParameterAttributes.None, bitStreamType));
            readMethod.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, new ByReferenceType(type)));
            
            readMethod.Body = new MethodBody(readMethod)
            {
                InitLocals = true
            };
            
            var read = readMethod.Body.GetILProcessor();
            GenerateMethod(false, readMethod, readMethodP, type, read, mainmodule, valueArg);
            serializerClass.Methods.Add(readMethod);
            
            GenerateDeltaSerializersProcessor.HandleType(assembly, type, serializerClass, messages);
            RegisterSerializersProcessor.HandleType(type.Module, serializerClass, isEditor, messages);
        }

        private static void HandleHashOnly(AssemblyDefinition assembly, TypeReference type, TypeDefinition serializerClass, bool isEditor)
        {
            var registerMethod = new MethodDefinition("Register", MethodAttributes.Static, assembly.MainModule.TypeSystem.Void);
            
            var attributeType = assembly.MainModule.GetTypeDefinition<RuntimeInitializeOnLoadMethodAttribute>(); 
            var constructor = attributeType.Resolve().Methods.First(m => m.IsConstructor && m.HasParameters).Import(assembly.MainModule);
            var attribute = new CustomAttribute(constructor);
            registerMethod.CustomAttributes.Add(attribute);

            if (isEditor)
            {
                var editorType = assembly.MainModule.GetTypeDefinition<RegisterPackersAttribute>().Import(assembly.MainModule);
                var editorConstructor = editorType.Resolve().Methods.First(m => m.IsConstructor && !m.HasParameters).Import(assembly.MainModule);
                var editorAttribute = new CustomAttribute(editorConstructor);
                registerMethod.CustomAttributes.Add(editorAttribute);
            }
            
            attribute.ConstructorArguments.Add(new CustomAttributeArgument(assembly.MainModule.TypeSystem.Int32, (int)RuntimeInitializeLoadType.AfterAssembliesLoaded));
            registerMethod.Body = new MethodBody(registerMethod)
            {
                InitLocals = true
            };
            
            var il = registerMethod.Body.GetILProcessor();
            
            var networkRegister = assembly.MainModule.GetTypeDefinition(typeof(NetworkRegister)).Import(assembly.MainModule);
            var hashMethod = networkRegister.GetMethod("Hash").Import(assembly.MainModule);
            
            // NetworkRegister.Hash(RuntimeTypeHandle handle);
            il.Emit(OpCodes.Ldtoken, type);
            il.Emit(OpCodes.Call, hashMethod);
            il.Emit(OpCodes.Ret);
            
            serializerClass.Methods.Add(registerMethod);
        }

        private static void HandleNetworkIdentity(AssemblyDefinition assembly, TypeReference type,
            TypeDefinition serializerClass, bool isEditor)
        {
            var registerMethod = new MethodDefinition("Register", MethodAttributes.Static, assembly.MainModule.TypeSystem.Void);
            
            var attributeType = assembly.MainModule.GetTypeDefinition<RuntimeInitializeOnLoadMethodAttribute>(); 
            var constructor = attributeType.Resolve().Methods.First(m => m.IsConstructor && m.HasParameters).Import(assembly.MainModule);
            var attribute = new CustomAttribute(constructor);
            registerMethod.CustomAttributes.Add(attribute);
            
            if (isEditor)
            {
                var editorType = assembly.MainModule.GetTypeDefinition<RegisterPackersAttribute>();
                var editorConstructor = editorType.Resolve().Methods.First(m => m.IsConstructor && !m.HasParameters).Import(assembly.MainModule);
                var editorAttribute = new CustomAttribute(editorConstructor);
                registerMethod.CustomAttributes.Add(editorAttribute);
            }
            
            attribute.ConstructorArguments.Add(new CustomAttributeArgument(assembly.MainModule.TypeSystem.Int32, (int)RuntimeInitializeLoadType.AfterAssembliesLoaded));
            registerMethod.Body = new MethodBody(registerMethod)
            {
                InitLocals = true
            };
            
            var register = registerMethod.Body.GetILProcessor();
            GenerateRegisterMethodForIdentity(type, register);
            serializerClass.Methods.Add(registerMethod);
        }
        
        private static void HandleNetworkModule(AssemblyDefinition assembly, TypeReference type,
            TypeDefinition serializerClass, bool isEditor)
        {
            var registerMethod = new MethodDefinition("Register", MethodAttributes.Static, assembly.MainModule.TypeSystem.Void);
            var attributeType = assembly.MainModule.GetTypeDefinition<RuntimeInitializeOnLoadMethodAttribute>(); 
            var constructor = attributeType.Resolve().Methods.First(m => m.IsConstructor && m.HasParameters).Import(assembly.MainModule);
            var attribute = new CustomAttribute(constructor);
            registerMethod.CustomAttributes.Add(attribute);
            if (isEditor)
            {
                var editorType = assembly.MainModule.GetTypeDefinition<RegisterPackersAttribute>();
                var editorConstructor = editorType.Resolve().Methods.First(m => m.IsConstructor && !m.HasParameters).Import(assembly.MainModule);
                var editorAttribute = new CustomAttribute(editorConstructor);
                registerMethod.CustomAttributes.Add(editorAttribute);
            }
            attribute.ConstructorArguments.Add(new CustomAttributeArgument(assembly.MainModule.TypeSystem.Int32, (int)RuntimeInitializeLoadType.AfterAssembliesLoaded));
            registerMethod.Body = new MethodBody(registerMethod)
            {
                InitLocals = true
            };
            
            var register = registerMethod.Body.GetILProcessor();
            GenerateRegisterMethodForModule(type, register);
            serializerClass.Methods.Add(registerMethod);
        }

        private static void HandleGenerics(AssemblyDefinition assembly, TypeReference type, HandledGenericTypes genericT,
            TypeDefinition serializerClass, bool isEditor)
        {
            var registerMethod = new MethodDefinition("Register", MethodAttributes.Static, assembly.MainModule.TypeSystem.Void);
            var attributeType = assembly.MainModule.GetTypeDefinition<RuntimeInitializeOnLoadMethodAttribute>(); 
            var constructor = attributeType.Resolve().Methods.First(m => m.IsConstructor && m.HasParameters).Import(assembly.MainModule);
            var attribute = new CustomAttribute(constructor);
            registerMethod.CustomAttributes.Add(attribute);
            if (isEditor)
            {
                var editorType = assembly.MainModule.GetTypeDefinition<RegisterPackersAttribute>();
                var editorConstructor = editorType.Resolve().Methods.First(m => m.IsConstructor && !m.HasParameters).Import(assembly.MainModule);
                var editorAttribute = new CustomAttribute(editorConstructor);
                registerMethod.CustomAttributes.Add(editorAttribute);
            }
            attribute.ConstructorArguments.Add(new CustomAttributeArgument(assembly.MainModule.TypeSystem.Int32, (int)RuntimeInitializeLoadType.AfterAssembliesLoaded));
            registerMethod.Body = new MethodBody(registerMethod)
            {
                InitLocals = true
            };
            
            var register = registerMethod.Body.GetILProcessor();
            GenerateRegisterMethod(assembly.MainModule, type, register, genericT);
            serializerClass.Methods.Add(registerMethod);
        }

        private static void GenerateRegisterMethodForIdentity(TypeReference type, ILProcessor il)
        {
            var packType = type.Module.GetTypeDefinition(typeof(PackNetworkIdentity));
            var registerMethod = packType.GetMethod("RegisterIdentity", true).Import(type.Module);
            
            var genericRegisterMethod = new GenericInstanceMethod(registerMethod);
            genericRegisterMethod.GenericArguments.Add(type);
            
            il.Emit(OpCodes.Call, genericRegisterMethod);
            il.Emit(OpCodes.Ret);
        }
        
        private static void GenerateRegisterMethodForModule(TypeReference type, ILProcessor il)
        {
            var packType = type.Module.GetTypeDefinition(typeof(PackNetworkModule));
            var registerMethod = packType.GetMethod("RegisterNetworkModule", true).Import(type.Module);
            
            var genericRegisterMethod = new GenericInstanceMethod(registerMethod);
            genericRegisterMethod.GenericArguments.Add(type);
            
            il.Emit(OpCodes.Call, genericRegisterMethod);
            il.Emit(OpCodes.Ret);
        }

        private static void GenerateRegisterMethod(ModuleDefinition module, TypeReference type, ILProcessor il, HandledGenericTypes handledType)
        {
            var importedType = type.Import(module);
            var packCollectionsType = module.GetTypeDefinition(typeof(PackCollections)).Import(module);
            
            switch (handledType)
            {
                case HandledGenericTypes.List when importedType is GenericInstanceType listType:
                    
                    var registerListMethod = packCollectionsType.GetMethod("RegisterList", true).Import(module);
                    var genericRegisterListMethod = new GenericInstanceMethod(registerListMethod);
                    genericRegisterListMethod.GenericArguments.Add(listType.GenericArguments[0]);
                    
                    il.Emit(OpCodes.Call, genericRegisterListMethod);
                    
                    break;
                case HandledGenericTypes.Array when importedType is ArrayType arrayType:
                    
                    var registerArrayMethod = packCollectionsType.GetMethod("RegisterArray", true).Import(module);
                    var genericRegisterArrayMethod = new GenericInstanceMethod(registerArrayMethod);
                    genericRegisterArrayMethod.GenericArguments.Add(arrayType.ElementType);
                    
                    il.Emit(OpCodes.Call, genericRegisterArrayMethod);
                    break;
                case HandledGenericTypes.HashSet when importedType is GenericInstanceType hashSetType:
                    
                    var registerHashSetMethod = packCollectionsType.GetMethod("RegisterHashSet", true).Import(module);
                    var genericRegisterHashSetMethod = new GenericInstanceMethod(registerHashSetMethod);
                    genericRegisterHashSetMethod.GenericArguments.Add(hashSetType.GenericArguments[0]);
                    
                    il.Emit(OpCodes.Call, genericRegisterHashSetMethod);
                    break;
                case HandledGenericTypes.Queue when importedType is GenericInstanceType queueType:
                    
                    var registerQueueMethod = packCollectionsType.GetMethod("RegisterQueue", true).Import(module);
                    var genericregisterQueueMethod = new GenericInstanceMethod(registerQueueMethod);
                    genericregisterQueueMethod.GenericArguments.Add(queueType.GenericArguments[0]);
                    
                    il.Emit(OpCodes.Call, genericregisterQueueMethod);
                    break;
                case HandledGenericTypes.Stack when importedType is GenericInstanceType stackType:
                    
                    var registerStackMethod = packCollectionsType.GetMethod("RegisterStack", true).Import(module);
                    var genericRegisterStackMethod = new GenericInstanceMethod(registerStackMethod);
                    genericRegisterStackMethod.GenericArguments.Add(stackType.GenericArguments[0]);
                    
                    il.Emit(OpCodes.Call, genericRegisterStackMethod);
                    break;
                case HandledGenericTypes.DisposableList when importedType is GenericInstanceType stackType:
                    
                    var registerDisposableListMethod = packCollectionsType.GetMethod("RegisterDisposableList", true).Import(module);
                    var genericRegisterDListMethod = new GenericInstanceMethod(registerDisposableListMethod);
                    genericRegisterDListMethod.GenericArguments.Add(stackType.GenericArguments[0]);
                    
                    il.Emit(OpCodes.Call, genericRegisterDListMethod);
                    break;
                case HandledGenericTypes.DisposableHashSet when importedType is GenericInstanceType stackType:
                    
                    var registerDisposableHashSetMethod = packCollectionsType.GetMethod("RegisterDisposableHashSet", true).Import(module);
                    var genericRegisterDSetMethod = new GenericInstanceMethod(registerDisposableHashSetMethod);
                    genericRegisterDSetMethod.GenericArguments.Add(stackType.GenericArguments[0]);
                    il.Emit(OpCodes.Call, genericRegisterDSetMethod);
                    break;
                case HandledGenericTypes.Dictionary when importedType is GenericInstanceType dictionaryType:
                    
                    var registerDictionaryMethod = packCollectionsType.GetMethod("RegisterDictionary", true).Import(module);
                    var genericRegisterDictionaryMethod = new GenericInstanceMethod(registerDictionaryMethod);
                    genericRegisterDictionaryMethod.GenericArguments.Add(dictionaryType.GenericArguments[0]);
                    genericRegisterDictionaryMethod.GenericArguments.Add(dictionaryType.GenericArguments[1]);
                    
                    il.Emit(OpCodes.Call, genericRegisterDictionaryMethod);
                    break;
                case HandledGenericTypes.Nullable when importedType is GenericInstanceType nullableType:
                    var registerNullableMethod = packCollectionsType.GetMethod("RegisterNullable", true).Import(module);
                    var genericRegisterNullableMethod = new GenericInstanceMethod(registerNullableMethod);
                    genericRegisterNullableMethod.GenericArguments.Add(nullableType.GenericArguments[0]);
                    
                    il.Emit(OpCodes.Call, genericRegisterNullableMethod);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(handledType), handledType, null);
            }
            
            il.Emit(OpCodes.Ret);
        }
        
        public static bool HasInterface(TypeDefinition def, Type interfaceType)
        {
            return def.Interfaces.Any(i => i.InterfaceType.FullName == interfaceType.FullName);
        }

        private static MethodReference CreateSetterMethod(TypeDefinition parent, FieldDefinition field)
        {
            var name = MakeFullNameValidCSharp($"Purrnet_Set_{field.Name}");
            
            foreach (var m in parent.Methods)
            {
                if (m.Name == name)
                    return m;
            }
            
            var method = new MethodDefinition(name, MethodAttributes.Public, parent.Module.TypeSystem.Void);
            var valueArg = new ParameterDefinition("value", ParameterAttributes.None, field.FieldType);
            method.Parameters.Add(valueArg);
            
            var setter = method.Body.GetILProcessor();
            
            FieldReference fieldRef;
            
            if (parent.HasGenericParameters)
            {
                // Link the field to the open generic instance
                var resolvedParent = new GenericInstanceType(parent);

                // Populate the generic arguments
                foreach (var genericArg in parent.GenericParameters)
                {
                    resolvedParent.GenericArguments.Add(genericArg);
                }

                // Create the FieldReference with the resolved generic parent
                fieldRef = new FieldReference(field.Name, field.FieldType, resolvedParent);
            }
            else
            {
                // Use the field directly if no generics are involved
                fieldRef = field;
            }
            
            setter.Emit(OpCodes.Ldarg_0);
            setter.Emit(OpCodes.Ldarg_1);
            setter.Emit(OpCodes.Stfld, fieldRef);
            
            setter.Emit(OpCodes.Ret);
            
            parent.Methods.Add(method);
            return method;
        }
        
        private static MethodReference CreateGetterMethod(TypeDefinition parent, FieldDefinition field)
        {
            var name = MakeFullNameValidCSharp($"Purrnet_Get_{field.Name}");
            
            foreach (var m in parent.Methods)
            {
                if (m.Name == name)
                    return m;
            }
            
            var method = new MethodDefinition(MakeFullNameValidCSharp($"Purrnet_Get_{field.Name}"), MethodAttributes.Public, field.FieldType);
            var getter = method.Body.GetILProcessor();
            
            FieldReference fieldRef;
            
            if (parent.HasGenericParameters)
            {
                // Link the field to the open generic instance
                var resolvedParent = new GenericInstanceType(parent);

                // Populate the generic arguments
                foreach (var genericArg in parent.GenericParameters)
                {
                    resolvedParent.GenericArguments.Add(genericArg);
                }

                // Create the FieldReference with the resolved generic parent
                fieldRef = new FieldReference(field.Name, field.FieldType, resolvedParent);
            }
            else
            {
                // Use the field directly if no generics are involved
                fieldRef = field;
            }

            getter.Emit(OpCodes.Ldarg_0); // Load "this"
            getter.Emit(OpCodes.Ldfld, fieldRef); // Load field in the correct generic context
            getter.Emit(OpCodes.Ret); // Return the field value
            
            parent.Methods.Add(method);
            return method;
        }
        
        private static void GenerateMethod(
            bool isWriting, MethodDefinition method, MethodReference serialize, TypeReference typeRef, ILProcessor il,
            ModuleDefinition mainmodule, ParameterDefinition valueArg)
        {
            var bitPackerType = mainmodule.GetTypeDefinition(typeof(BitPacker)).Import(mainmodule);
            var packerType = mainmodule.GetTypeDefinition(typeof(Packer<>)).Import(mainmodule);
            var type = typeRef.Resolve();
            
            var writeIsNull = bitPackerType.GetMethod("WriteIsNull", true).Import(mainmodule);
            var readIsNull = bitPackerType.GetMethod("ReadIsNull", true).Import(mainmodule);
            bool isClass = !type.IsValueType;

            var ret = il.Create(OpCodes.Ret);

            if (isClass)
            {
                // write null check
                if (isWriting)
                {
                    var genericIsNull = new GenericInstanceMethod(writeIsNull);
                    genericIsNull.GenericArguments.Add(typeRef);
                    
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Call, genericIsNull);
                }
                else
                {
                    var genericIsNull = new GenericInstanceMethod(readIsNull);
                    genericIsNull.GenericArguments.Add(typeRef);
                    
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Call, genericIsNull);
                }
                
                // if returned false, just return
                il.Emit(OpCodes.Brfalse, ret);
            }
            
            if (type.IsEnum)
            {
                HandleEnums(isWriting, method, serialize, type, il, packerType, mainmodule);
                return;
            }

            if (HasInterface(type, typeof(IPacked)))
            {
                HandleIData(isWriting, type, il, mainmodule, valueArg);
                return;
            }
            
            if (HasInterface(type, typeof(IPackedSimple)))
            {
                HandleIDataSimple(isWriting, type, il, mainmodule, valueArg);
                return;
            }
            
            foreach (var field in type.Fields)
            {
                if (field.IsStatic)
                    continue;
                
                bool isDelegate = PostProcessor.InheritsFrom(field.FieldType.Resolve(), typeof(Delegate).FullName);
            
                if (isDelegate)
                    continue;

                var fieldType = ResolveGenericFieldType(field, typeRef);
                var genericM = CreateGenericMethod(packerType, fieldType, serialize, mainmodule);
                
                // make field public
                if (!field.IsPublic)
                {
                    if (isWriting)
                    {
                        var getter = CreateGetterMethod(type, field);

                        if (typeRef is GenericInstanceType genericInstanceType)
                        {
                            var copy = new MethodReference(getter.Name, getter.ReturnType, genericInstanceType)
                            {
                                HasThis = getter.HasThis
                            };

                            foreach (var param in getter.Parameters)
                                copy.Parameters.Add(new ParameterDefinition(param.Name, param.Attributes, param.ParameterType));
                        
                            foreach (var genericParam in getter.GenericParameters)
                                copy.GenericParameters.Add(new GenericParameter(genericParam.Name, copy));
                            
                            getter = copy;
                        }
                        
                        il.Emit(OpCodes.Ldarg_0);
                        
                        if (isClass)
                            il.Emit(OpCodes.Ldarg_1);
                        else
                            il.Emit(OpCodes.Ldarga_S, valueArg);

                        il.Emit(OpCodes.Call, getter);
                        il.Emit(OpCodes.Call, genericM);
                    }
                    else
                    {
                        var variable = new VariableDefinition(fieldType);
                        method.Body.Variables.Add(variable);
                        
                        var setter = CreateSetterMethod(type, field);

                        if (typeRef is GenericInstanceType genericInstanceType)
                        {
                            var copy = new MethodReference(setter.Name, setter.ReturnType, genericInstanceType)
                            {
                                HasThis = setter.HasThis
                            };
                            
                            foreach (var param in setter.Parameters)
                                copy.Parameters.Add(new ParameterDefinition(param.Name, param.Attributes, param.ParameterType));
                        
                            foreach (var genericParam in setter.GenericParameters)
                                copy.GenericParameters.Add(new GenericParameter(genericParam.Name, copy));
                            
                            setter = copy;
                        }
                        
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldloca, variable);
                        il.Emit(OpCodes.Call, genericM);
                        
                        il.Emit(OpCodes.Ldarg_1);
                        if (isClass) il.Emit(OpCodes.Ldind_Ref);

                        il.Emit(OpCodes.Ldloc, variable);
                        il.Emit(OpCodes.Call, setter);
                    }
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    if (isClass && !isWriting) il.Emit(OpCodes.Ldind_Ref);
                    
                    var fieldRef = new FieldReference(field.Name, field.FieldType, typeRef).Import(mainmodule);
                    
                    il.Emit(isWriting ? OpCodes.Ldfld : OpCodes.Ldflda, fieldRef);
                    il.Emit(OpCodes.Call, genericM);
                }

            }
            
            /*il.Emit(OpCodes.Ldarg_S, valueArg);
            if (isClass) il.Emit(OpCodes.Ldind_Ref);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, readData);*/
            
            il.Append(ret);
        }
        
        public static TypeReference ResolveGenericFieldType(FieldDefinition field, TypeReference declaringType)
        {
            var fieldType = field.FieldType;

            // If the declaring type is a generic instance
            if (declaringType is GenericInstanceType genericDeclaringType)
            {
                // If the field type has generic parameters
                if (fieldType.IsGenericParameter)
                {
                    // Map the generic parameter to the actual type argument
                    var genericParam = (GenericParameter)fieldType;
                    int position = genericParam.Position;
                    return genericDeclaringType.GenericArguments[position];
                }

                // If the field type is itself a generic instance
                if (fieldType is GenericInstanceType fieldGenericInstance)
                {
                    var resolvedInstance = new GenericInstanceType(fieldGenericInstance.ElementType);
                    foreach (var arg in fieldGenericInstance.GenericArguments)
                    {
                        if (arg.IsGenericParameter)
                        {
                            var genericParam = (GenericParameter)arg;
                            int position = genericParam.Position;
                            resolvedInstance.GenericArguments.Add(genericDeclaringType.GenericArguments[position]);
                        }
                        else
                        {
                            resolvedInstance.GenericArguments.Add(arg);
                        }
                    }
                    return resolvedInstance;
                }
            }

            // Return the original field type if no generics are involved
            return fieldType;
        }

        private static void HandleIData(bool isWriting, TypeDefinition type,
            ILProcessor il, ModuleDefinition mainmodule, ParameterDefinition valueArg)
        {
            bool isClass = !type.IsValueType;

            if (isWriting)
            {
                var writeData = type.GetMethod("Write").Import(mainmodule);
                il.Emit(isClass ? OpCodes.Ldarg : OpCodes.Ldarga_S, valueArg);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, writeData);
            }
            else
            {
                var readData = type.GetMethod("Read").Import(mainmodule);
                il.Emit(OpCodes.Ldarg_S, valueArg);
                if (isClass) il.Emit(OpCodes.Ldind_Ref);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, readData);
            }
                
            il.Emit(OpCodes.Ret);
        }
        
        private static void HandleIDataSimple(bool isWriting, TypeDefinition type,
            ILProcessor il, ModuleDefinition mainmodule, ParameterDefinition valueArg)
        {
            bool isClass = !type.IsValueType;
            if (isWriting)
            {
                var writeData = type.GetMethod("Serialize").Import(mainmodule);
                il.Emit(isClass ? OpCodes.Ldarg : OpCodes.Ldarga_S, valueArg);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, writeData);
            }
            else
            {
                var readData = type.GetMethod("Serialize").Import(mainmodule);
                il.Emit(OpCodes.Ldarg_S, valueArg);
                if (isClass) il.Emit(OpCodes.Ldind_Ref);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, readData);
            }
                
            il.Emit(OpCodes.Ret);
        }

        private static void HandleEnums(bool isWriting, MethodDefinition method, MethodReference serialize,
            TypeDefinition type, ILProcessor il, TypeReference packerType, ModuleDefinition mainmodule)
        {
            var underlyingType = type.GetField("value__").FieldType;
            var enumWriteMethod = CreateGenericMethod(packerType, underlyingType, serialize, mainmodule);
                
            var tmpVar = new VariableDefinition(underlyingType);
                
            if (!isWriting)
            {
                method.Body.Variables.Add(tmpVar);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stloc_0);
            }
                
            il.Emit(OpCodes.Ldarg_0);

            // load the address of the field
            if (isWriting)
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, enumWriteMethod);
            }
            else
            {
                il.Emit(OpCodes.Ldloca, tmpVar);
                il.Emit(OpCodes.Call, enumWriteMethod);

                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldloc_0);
                EmitStindForEnum(il, type);
            }
                
            il.Emit(OpCodes.Ret);
        }

        public static MethodReference CreateGenericMethod(TypeReference packerType, TypeReference type,
            MethodReference writeMethod, ModuleDefinition mainmodule)
        {
            var genericPackerType = new GenericInstanceType(packerType);
            genericPackerType.GenericArguments.Add(type);
                
            var genericWriteMethod = new MethodReference(writeMethod.Name, writeMethod.ReturnType, genericPackerType.Import(mainmodule))
            {
                HasThis = writeMethod.HasThis
            }; 
                
            foreach (var param in writeMethod.Parameters)
                genericWriteMethod.Parameters.Add(new ParameterDefinition(param.Name, param.Attributes, param.ParameterType));
            return genericWriteMethod.Import(mainmodule);
        }

        private static bool IsGeneric(TypeReference typeDef, out HandledGenericTypes type)
        {        
            if (typeDef.IsArray)
            {
                type = HandledGenericTypes.Array; 
                return true;
            }
            
            if (IsGeneric(typeDef, typeof(DisposableList<>)))
            {
                type = HandledGenericTypes.DisposableList;
                return true;
            }
            
            if (IsGeneric(typeDef, typeof(DisposableHashSet<>)))
            {
                type = HandledGenericTypes.DisposableHashSet;
                return true;
            }
            
            if (IsGeneric(typeDef, typeof(List<>)))
            {
                type = HandledGenericTypes.List;
                return true;
            }
            
            if (IsGeneric(typeDef, typeof(Queue<>)))
            {
                type = HandledGenericTypes.Queue;
                return true;
            }
            
            if (IsGeneric(typeDef, typeof(Stack<>)))
            {
                type = HandledGenericTypes.Stack;
                return true;
            }
            
            if (IsGeneric(typeDef, typeof(HashSet<>)))
            {
                type = HandledGenericTypes.HashSet;
                return true;
            }
            
            if (IsGeneric(typeDef, typeof(Dictionary<,>)))
            {
                type = HandledGenericTypes.Dictionary;
                return true;
            }
            
            if (IsGeneric(typeDef, typeof(Nullable<>)))
            {
                type = HandledGenericTypes.Nullable;
                return true;
            }

            type = HandledGenericTypes.None;
            return false;
        }
        
        private static bool IsGeneric(TypeReference typeDef, Type type)
        {
            // Ensure method has a generic return type
            if (typeDef is GenericInstanceType genericReturnType)
            {
                // Resolve the element type to compare against Task<>
                var resolvedType = genericReturnType.ElementType.Resolve();

                // Check if the resolved type matches Task<>
                return resolvedType != null && resolvedType.FullName == type.FullName;
            }

            return false;
        }
        
        private static TypeReference GetEnumUnderlyingType(TypeDefinition typeDef)
        {
            if (!typeDef.IsEnum)
                return null;

            var valueField = typeDef.Fields.FirstOrDefault(f => f.Name == "value__");
            return valueField?.FieldType;
        }
        
        private static void EmitStindForEnum(ILProcessor il, TypeDefinition enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException($"{enumType.FullName} is not an enum.");
            }

            // Get the underlying type of the enum
            var underlyingType = GetEnumUnderlyingType(enumType);

            if (underlyingType == null)
            {
                throw new InvalidOperationException($"Unable to determine the underlying type of the enum {enumType.FullName}.");
            }

            // Emit the appropriate Stind opcode based on the underlying type
            switch (underlyingType.FullName)
            {
                case "System.Byte":
                case "System.SByte":
                    il.Emit(OpCodes.Stind_I1); // Store value for 1-byte types
                    break;

                case "System.Int16":
                case "System.UInt16":
                    il.Emit(OpCodes.Stind_I2); // Store value for 2-byte types
                    break;

                case "System.Int32":
                case "System.UInt32":
                    il.Emit(OpCodes.Stind_I4); // Store value for 4-byte types
                    break;

                case "System.Int64":
                case "System.UInt64":
                    il.Emit(OpCodes.Stind_I8); // Store value for 8-byte types
                    break;

                default:
                    throw new NotSupportedException($"Unsupported enum underlying type: {underlyingType.FullName}");
            }
        }
    }
}

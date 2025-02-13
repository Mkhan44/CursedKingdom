using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using PurrNet.Packing;
using Unity.CompilationPipeline.Common.Diagnostics;
using UnityEngine;

namespace PurrNet.Codegen
{
    public static class RegisterSerializersProcessor
    {
        // static void WriteHalf(BitPacker packer, Half oldvalue, Half newvalue)
        static bool IsDeltaWriteMethod(MethodDefinition method, out TypeReference type)
        {
            type = null;
            
            if (method.Parameters.Count != 3)
                return false;

            if (method.Parameters[0].ParameterType.FullName != typeof(BitPacker).FullName)
                return false;
            
            if (method.Parameters[1].ParameterType.IsByReference)
                return false;
            
            if (method.Parameters[2].ParameterType.IsByReference)
                return false;

            type = method.Parameters[1].ParameterType;
            return true;
        }
        
        // static void ReadHalf(BitPacker packer, Half oldvalue, ref Half value)
        static bool IsDeltaReadMethod(MethodDefinition method, out TypeReference type)
        {
            type = null;
            
            if (method.Parameters.Count != 3)
                return false;

            if (method.Parameters[0].ParameterType.FullName != typeof(BitPacker).FullName)
                return false;
            
            if (method.Parameters[1].ParameterType.IsByReference)
                return false;
            
            if (!method.Parameters[2].ParameterType.IsByReference)
                return false;
            
            type = method.Parameters[1].ParameterType;

            return true;
        }
        
        static bool IsWriteMethod(MethodDefinition method, out TypeReference type)
        {
            type = null;

            /*if (method.HasGenericParameters || method.ContainsGenericParameter)
                return false;*/
            
            if (method.Parameters.Count != 2)
                return false;

            if (method.Parameters[0].ParameterType.FullName != typeof(BitPacker).FullName)
                return false;
            
            if (method.Parameters[1].ParameterType.IsByReference)
                return false;

            type = method.Parameters[1].ParameterType;
            return true;
        }
        
        static bool IsReadMethod(MethodDefinition method, out TypeReference type)
        {
            type = null;
            
            /*if (method.HasGenericParameters || method.ContainsGenericParameter)
                return false;*/

            if (method.Parameters.Count != 2)
                return false;

            if (method.Parameters[0].ParameterType.FullName != typeof(BitPacker).FullName)
                return false;
            
            if (!method.Parameters[1].ParameterType.IsByReference)
                return false;
            
            type = method.Parameters[1].ParameterType;

            return true;
        }

        struct PackType
        {
            public bool isDelta;
            public TypeReference type;
            public MethodDefinition method;
        }
        
        public static void HandleType(ModuleDefinition module, TypeDefinition type, bool isEditor, List<DiagnosticMessage> messages)
        {
            if (type.FullName == typeof(Packer).FullName) 
                return;
            
            if (type.FullName == typeof(Packer<>).FullName) 
                return;
            
            bool isStatic = type.IsAbstract && type.IsSealed;
            
            if (!isStatic)
                return;
            
            List<PackType> writeTypes = new List<PackType>();
            List<PackType> readTypes = new List<PackType>();

            foreach (var method in type.Methods)
            {
                if (method.HasGenericParameters || method.ContainsGenericParameter)
                    continue;
                
                // Skip non-static classes
                if (!method.IsStatic)
                    break;
                
                if (method.HasGenericParameters)
                    continue;
                
                if (IsWriteMethod(method, out var writeType))
                {
                    if (writeType == null)
                        throw new Exception("WriteType is null");
                    
                    writeTypes.Add(new PackType
                    {
                        type = writeType,
                        method = method
                    });
                }
                else if (IsReadMethod(method, out var readType))
                {
                    if (readType == null)
                        throw new Exception("ReadType is null");
                    
                    readTypes.Add(new PackType
                    {
                        type = readType,
                        method = method
                    });
                }
                else if (IsDeltaWriteMethod(method, out var deltaWriteType))
                {
                    if (deltaWriteType == null)
                        throw new Exception("DeltaWriteType is null");
                    
                    writeTypes.Add(new PackType
                    {
                        isDelta = true,
                        type = deltaWriteType,
                        method = method
                    });
                }
                else if (IsDeltaReadMethod(method, out var deltaReadType))
                {
                    if (deltaReadType == null)
                        throw new Exception("DeltaReadType is null");
                    
                    readTypes.Add(new PackType
                    {
                        isDelta = true,
                        type = deltaReadType,
                        method = method
                    });
                }
            }
            
            if (writeTypes.Count == 0 && readTypes.Count == 0)
                return;
            
            var writeFuncDelegate = module.GetTypeDefinition(typeof(WriteFunc<>)).Import(module);
            var readFuncDelegate = module.GetTypeDefinition(typeof(ReadFunc<>)).Import(module);
            
            var deltaWriteFuncDelegate = module.GetTypeDefinition(typeof(DeltaWriteFunc<>)).Import(module);
            var deltaReadFuncDelegate = module.GetTypeDefinition(typeof(DeltaReadFunc<>)).Import(module);
            
            var registerMethod = new MethodDefinition("Register_Type_Generated_PurrNet", MethodAttributes.Static, module.TypeSystem.Void);
            var attributeType = module.GetTypeDefinition<RuntimeInitializeOnLoadMethodAttribute>(); 
            var constructor = attributeType.Resolve().Methods.First(m => m.IsConstructor && m.HasParameters).Import(module);
            var attribute = new CustomAttribute(constructor);
            
            if (isEditor)
            {
                var editorType = module.GetTypeDefinition<RegisterPackersAttribute>().Import(module);
                var editorConstructor = editorType.Resolve().Methods.First(m => m.IsConstructor && !m.HasParameters).Import(module);
                var editorAttribute = new CustomAttribute(editorConstructor);
                registerMethod.CustomAttributes.Add(editorAttribute);
            }
            
            registerMethod.CustomAttributes.Add(attribute);
            attribute.ConstructorArguments.Add(new CustomAttributeArgument(module.TypeSystem.Int32, (int)RuntimeInitializeLoadType.AfterAssembliesLoaded));
            
            registerMethod.Body.InitLocals = true;
            
            type.Methods.Add(registerMethod);
            
            var il = registerMethod.Body.GetILProcessor();
            
            for (int i = 0; i < writeTypes.Count; i++)
            {
                var writeType = writeTypes[i];
                var writeMethod = writeType.method.Import(module);
                
                writeMethod.Resolve().AggressiveInlining = true;
                
                var nonDeltaPackerType = module.GetTypeDefinition(typeof(Packer<>)).Import(module);
                var deltaPackerType = module.GetTypeDefinition(typeof(DeltaPacker<>)).Import(module);
                
                var actualPackerType = writeType.isDelta ? deltaPackerType : nonDeltaPackerType;
                
                var genPackerType = new GenericInstanceType(actualPackerType);
                genPackerType.GenericArguments.Add(writeType.type.Import(module));

                var writeDelType = writeType.isDelta ? deltaWriteFuncDelegate : writeFuncDelegate;
                var writeFuncGeneric = new GenericInstanceType(writeDelType);
                writeFuncGeneric.GenericArguments.Add(writeType.type.Import(module));
                
                var delegateConstructor = writeDelType.Resolve()
                    .Methods.First(m => m.IsConstructor && m.HasParameters);
                var delegateConstructorRef = new MethodReference(delegateConstructor.Name, delegateConstructor.ReturnType, writeFuncGeneric)
                {
                    HasThis = delegateConstructor.HasThis,
                    ExplicitThis = delegateConstructor.ExplicitThis,
                    CallingConvention = delegateConstructor.CallingConvention
                };
                
                foreach (var param in delegateConstructor.Parameters)
                    delegateConstructorRef.Parameters.Add(new ParameterDefinition(param.Name, param.Attributes, param.ParameterType));
                
                // Packer.RegisterWriter<int>(Write);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Ldftn, writeMethod);
                il.Emit(OpCodes.Newobj, delegateConstructorRef);
                
                var write = genPackerType.GetMethod("RegisterWriter", false).Import(module);
                var genericWrite = new MethodReference("RegisterWriter", module.TypeSystem.Void, genPackerType)
                {
                    HasThis = write.HasThis,
                    ExplicitThis = write.ExplicitThis,
                    CallingConvention = write.CallingConvention
                };

                foreach (var param in write.Parameters)
                    genericWrite.Parameters.Add(new ParameterDefinition(param.Name, param.Attributes, param.ParameterType));
                
                il.Emit(OpCodes.Call, genericWrite);
            }
            
            for (int i = 0; i < readTypes.Count; i++)
            {
                var readType = readTypes[i];
                var readMethod = readType.method.Import(module);
                
                readMethod.Resolve().AggressiveInlining = true;

                // Create a GenericInstanceMethod for Packer.RegisterReader<T>
                
                var typeArgument = readType.type.Import(module);
                
                // If the type is a ByReferenceType (e.g., ref int), get the base type
                if (typeArgument is ByReferenceType byRefType)
                {
                    typeArgument = byRefType.ElementType; // Use the base type (e.g., int from ref int)
                }
                
                var packerType = module.GetTypeDefinition(typeof(Packer<>)).Import(module);
                var deltaPackerType = module.GetTypeDefinition(typeof(DeltaPacker<>)).Import(module);
                var actualPackerType = readType.isDelta ? deltaPackerType : packerType;
                
                var genPackerType = new GenericInstanceType(actualPackerType);
                genPackerType.GenericArguments.Add(typeArgument);

                // Create the generic delegate type (ReadFunc<T>)
                var readDelType = readType.isDelta ? deltaReadFuncDelegate : readFuncDelegate;
                var readFuncGeneric = new GenericInstanceType(readDelType);
                readFuncGeneric.GenericArguments.Add(typeArgument);

                // Resolve the constructor of the generic delegate (ReadFunc<T>(object, IntPtr))
                var delegateConstructor = readDelType.Resolve()
                    .Methods.First(m => m.IsConstructor && m.HasParameters);
                
                // Construct the delegate constructor reference
                var delegateConstructorRef = new MethodReference(delegateConstructor.Name, delegateConstructor.ReturnType, readFuncGeneric)
                {
                    HasThis = delegateConstructor.HasThis,
                    ExplicitThis = delegateConstructor.ExplicitThis,
                    CallingConvention = delegateConstructor.CallingConvention
                };
                
                // Add parameters to the delegate constructor reference
                foreach (var param in delegateConstructor.Parameters)
                {
                    delegateConstructorRef.Parameters.Add(new ParameterDefinition(param.Name, param.Attributes, param.ParameterType));
                }

                // Generate IL for: Packer.RegisterReader<int>(Read);
                il.Emit(OpCodes.Ldnull);                     // Load 'null' for the target instance (static method)
                il.Emit(OpCodes.Ldftn, readMethod);          // Load the method pointer
                il.Emit(OpCodes.Newobj, delegateConstructorRef); // Create the delegate instance
                
                var read = genPackerType.GetMethod("RegisterReader", false).Import(module);
                var genericread = new MethodReference("RegisterReader", module.TypeSystem.Void, genPackerType)
                {
                    HasThis = read.HasThis,
                    ExplicitThis = read.ExplicitThis,
                    CallingConvention = read.CallingConvention
                };

                foreach (var param in read.Parameters)
                    genericread.Parameters.Add(new ParameterDefinition(param.Name, param.Attributes, param.ParameterType));
                
                il.Emit(OpCodes.Call, genericread);
            }
            
            il.Emit(OpCodes.Ret);
        }
    }
}

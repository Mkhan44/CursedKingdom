using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace PurrNet.Codegen
{
    public static class GenerateAutoMathProcessor
    {
        private static bool SkipType(TypeReference type)
        {
            if (type.FullName is "System.Int32" or "System.Int64" or "System.Int16")
                return true;
            
            return false;
        }
        
        private static bool IsPrimitiveNumeric(TypeReference type)
        {
            return type.FullName switch
            {
                "System.Single" => true,  // float
                "System.Double" => true,  // double
                "System.Int32" => true,   // int
                "System.Int64" => true,   // long
                "System.Int16" => true,   // short
                _ => false
            };
        }

        static MethodDefinition GetMathMethod(TypeDefinition type, string name, TypeReference math)
        {
            for (var i = 0; i < type.Methods.Count; i++)
            {
                var returnType = type.Methods[i].ReturnType;
                var parameters = type.Methods[i].Parameters;
                
                if (returnType.FullName != math.FullName)
                    continue;
                
                if (parameters.Count == 0)
                    continue;
                
                if (parameters[0].ParameterType.FullName != math.FullName)
                    continue;
                
                if (type.Methods[i].Name == name && type.Methods[i].HasGenericParameters == false)
                    return type.Methods[i];
            }

            return null;
        }
        
        public static void HandleType(TypeDefinition type, TypeReference math, TypeReference interf, List<DiagnosticMessage> messages)
        {
            if (math == null)
                return;
            
            if (!type.IsClass)
                return;
            
            if (type.IsAbstract)
                return;
            
            if (type.ContainsGenericParameter || type.HasGenericParameters)
                return;
            
            if (!type.IsValueType)
            {
                messages.Add(new DiagnosticMessage
                {
                    MessageData = $"Type `{type.FullName}` is a class and cannot be used as a math type, please use a struct instead.",
                    DiagnosticType = DiagnosticType.Warning
                });
                return;
            }
            
            var resolvedMath = math.Resolve();
            
            if (resolvedMath == null)
                return;
            
            if (type.FullName != resolvedMath.FullName)
            {
                messages.Add(new DiagnosticMessage
                {
                    MessageData = $"Type `{type.FullName}` does not match the math type `{resolvedMath.FullName}`.",
                    DiagnosticType = DiagnosticType.Warning
                });
                return;
            }

            var intMethodAttibutes = MethodAttributes.Public |
                                     MethodAttributes.Final | 
                                     MethodAttributes.HideBySig | 
                                     MethodAttributes.NewSlot |
                                     MethodAttributes.Virtual;
            
            var add = GetMathMethod(type, "Add", math);

            if (add == null)
            {
                add = new MethodDefinition("Add", intMethodAttibutes, math);
                add.Parameters.Add(new ParameterDefinition(math));
                add.Parameters.Add(new ParameterDefinition(math));

                /*var addMethod = interf.GetMethod("Add").Import(type.Module);
                var addMethodRef = new MethodReference(addMethod.Name, addMethod.ReturnType, type.Module.ImportReference(interf));
                add.Overrides.Add(addMethodRef);*/
                
                type.Methods.Add(add);
                HandleAdd(add, type);
            }
            
            var negate = GetMathMethod(type, "Negate", math);
            
            if (negate == null)
            {
                negate = new MethodDefinition("Negate", intMethodAttibutes, math);
                negate.Parameters.Add(new ParameterDefinition(math));
                
                /*var negateMethod = interf.GetMethod("Negate").Import(type.Module);
                var negateMethodRef = new MethodReference(negateMethod.Name, negateMethod.ReturnType, type.Module.ImportReference(interf));
                negate.Overrides.Add(negateMethodRef);*/
                
                type.Methods.Add(negate);
                HandleNegate(negate, type);
            }
            
            var scale = GetMathMethod(type, "Scale", math);
            
            if (scale == null)
            {
                scale = new MethodDefinition("Scale", intMethodAttibutes, math);
                scale.Parameters.Add(new ParameterDefinition(math));
                scale.Parameters.Add(new ParameterDefinition(type.Module.TypeSystem.Single));
                
                /*var scaleMethod = interf.GetMethod("Scale").Import(type.Module);
                var scaleMethodRef = new MethodReference(scaleMethod.Name, scaleMethod.ReturnType, type.Module.ImportReference(interf));
                scale.Overrides.Add(scaleMethodRef);*/
                
                type.Methods.Add(scale);
                HandleScale(scale, type);
            }
        }
        
        private static void HandleAdd(MethodDefinition method, TypeDefinition math)
        {
            var processor = method.Body.GetILProcessor();
            var variables = method.Body.Variables;
    
            var var0 = new VariableDefinition(math);
            variables.Add(var0);
    
            // Initialize the local variable
            processor.Emit(OpCodes.Ldarg_1);
            processor.Emit(OpCodes.Stloc_0);

            foreach (var field in math.Fields)
            {
                if (field.IsInitOnly)
                    continue;
                
                var fieldType = field.FieldType.Resolve();

                if (SkipType(fieldType))
                    continue;
                
                if (IsPrimitiveNumeric(field.FieldType))
                {
                    processor.Emit(OpCodes.Ldloca_S, var0);
            
                    processor.Emit(OpCodes.Ldarg_1);
                    processor.Emit(OpCodes.Ldfld, field);
            
                    processor.Emit(OpCodes.Ldarg_2);
                    processor.Emit(OpCodes.Ldfld, field);
            
                    processor.Emit(OpCodes.Add);
            
                    processor.Emit(OpCodes.Stfld, field);
                    continue;
                }
                
                var addOperator = fieldType.Methods.FirstOrDefault(m => 
                    m.Name == "op_Addition" && 
                    m.Parameters.Count == 2 && 
                    m.Parameters[0].ParameterType.FullName == fieldType.FullName &&
                    m.Parameters[1].ParameterType.FullName == fieldType.FullName);

                // Skip if no add operator found
                if (addOperator == null)
                    continue;
    
                // Load address of local variable for field access
                processor.Emit(OpCodes.Ldloca_S, var0);

                // Load first parameter and get its field
                processor.Emit(OpCodes.Ldarg_1);
                processor.Emit(OpCodes.Ldfld, field);

                // Load second parameter and get its field
                processor.Emit(OpCodes.Ldarg_2);
                processor.Emit(OpCodes.Ldfld, field);

                // Add the fields using the operator
                var importedAddOperator = method.Module.ImportReference(addOperator);
                processor.Emit(OpCodes.Call, importedAddOperator);

                // Store the result in the field of our new instance
                processor.Emit(OpCodes.Stfld, field);
            }

            // Load the local variable for return
            processor.Emit(OpCodes.Ldloc_0);
            processor.Emit(OpCodes.Ret);
        }

        private static void HandleNegate(MethodDefinition method, TypeDefinition math)
        {
            var processor = method.Body.GetILProcessor();
            var variables = method.Body.Variables;

            var var0 = new VariableDefinition(math);
            variables.Add(var0);

            processor.Emit(OpCodes.Ldarg_1);
            processor.Emit(OpCodes.Stloc_0);

            foreach (var field in math.Fields)
            {
                if (field.IsInitOnly)
                    continue;
                
                var fieldType = field.FieldType.Resolve();
                
                if (SkipType(fieldType))
                    continue;

                if (IsPrimitiveNumeric(field.FieldType))
                {
                    processor.Emit(OpCodes.Ldloca_S, var0);
            
                    processor.Emit(OpCodes.Ldarg_1);
                    processor.Emit(OpCodes.Ldfld, field);
            
                    processor.Emit(OpCodes.Neg);
            
                    processor.Emit(OpCodes.Stfld, field);
                    continue;
                }
                
                var negateOperator = fieldType.Methods.FirstOrDefault(m => 
                    m.Name == "op_UnaryNegation" && 
                    m.Parameters.Count == 1 && 
                    m.Parameters[0].ParameterType.FullName == fieldType.FullName);
                
                if (negateOperator == null)
                    continue;

                processor.Emit(OpCodes.Ldloca_S, var0);
                processor.Emit(OpCodes.Ldarg_1);
                processor.Emit(OpCodes.Ldfld, field);

                var importedNegateOperator = method.Module.ImportReference(negateOperator);
                processor.Emit(OpCodes.Call, importedNegateOperator);
                processor.Emit(OpCodes.Stfld, field);
            }

            processor.Emit(OpCodes.Ldloc_0);
            processor.Emit(OpCodes.Ret);
        }

        private static void HandleScale(MethodDefinition method, TypeDefinition math)
        {
            var processor = method.Body.GetILProcessor();
            var variables = method.Body.Variables;

            var var0 = new VariableDefinition(math);
            variables.Add(var0);
 
            processor.Emit(OpCodes.Ldarg_1);
            processor.Emit(OpCodes.Stloc_0);

            foreach (var field in math.Fields)
            {
                if (field.IsInitOnly)
                    continue;
                
                var fieldType = field.FieldType.Resolve();
                
                if (SkipType(fieldType))
                    continue;

                if (IsPrimitiveNumeric(field.FieldType))
                {
                    processor.Emit(OpCodes.Ldloca_S, var0);
            
                    processor.Emit(OpCodes.Ldarg_1);
                    processor.Emit(OpCodes.Ldfld, field);
            
                    processor.Emit(OpCodes.Ldarg_2);
            
                    processor.Emit(OpCodes.Mul);
            
                    processor.Emit(OpCodes.Stfld, field);
                    continue;
                }
                
                var scaleOperator = fieldType.Methods.FirstOrDefault(m => 
                    m.Name == "op_Multiply" && 
                    m.Parameters.Count == 2 && 
                    m.Parameters[0].ParameterType.FullName == fieldType.FullName &&
                    m.Parameters[1].ParameterType.FullName == "System.Single");
                    
                if (scaleOperator == null)
                    continue;

                processor.Emit(OpCodes.Ldloca_S, var0);
                processor.Emit(OpCodes.Ldarg_1);
                processor.Emit(OpCodes.Ldfld, field);
                processor.Emit(OpCodes.Ldarg_2);

                var importedScaleOperator = method.Module.ImportReference(scaleOperator);
                processor.Emit(OpCodes.Call, importedScaleOperator);
                processor.Emit(OpCodes.Stfld, field);
            }

            processor.Emit(OpCodes.Ldloc_0);
            processor.Emit(OpCodes.Ret);
        }
    }
}

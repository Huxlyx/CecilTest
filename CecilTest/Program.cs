using Mono.Cecil;
using Mono.Cecil.Cil;
using System;

namespace CecilTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AssemblyDefinition replaceAssembly = AssemblyDefinition.ReadAssembly(@"..\..\..\..\ReplaceProject\bin\Debug\net6.0\ReplaceProject.dll");
            AssemblyDefinition baseAssembly = AssemblyDefinition.ReadAssembly(@"..\..\..\..\BaseProject\bin\Debug\net6.0\BaseProject.dll"
//, new ReaderParameters() { 
//                AssemblyResolver = new HackyResolver(replaceAssembly)
//            }
);

            Console.WriteLine($"Base assembly {baseAssembly.FullName}");
            Console.WriteLine($"Replace assembly {replaceAssembly.FullName}");

            foreach (var module in baseAssembly.Modules)
            {
                Console.WriteLine(module + " in baseAssembly");
                Console.WriteLine(module.GetDebugHeader());

                List<TypeDefinition?> typesForRemoval = new();

                foreach (TypeDefinition? type in module.GetTypes())
                {
                    Console.WriteLine(" t: " + type?.FullName);

                    foreach (MethodDefinition? method in type.Methods)
                    {
                        Mono.Cecil.Cil.ILProcessor? il = method.Body.GetILProcessor();
                        Console.WriteLine(">>>  In method: " + method.FullName + "<<<<");
                        Console.WriteLine(">>>{");

                        HashSet<(TypeReference oldRef, TypeReference newRef)> typesNeedingReplacement = new HashSet<(TypeReference, TypeReference)>();
                        
                        foreach (Instruction? instruction in il.Body.Instructions)
                        {
                            if (instruction.OpCode == OpCodes.Newobj)
                            {
                                MethodDefinition? replacingMethod = FindMethodInAssembly(replaceAssembly, ((MethodDefinition)instruction.Operand).FullName);
                                Console.WriteLine($"Removing declaring type {((MethodDefinition)instruction.Operand).DeclaringType.FullName}");
                                TypeDefinition? declaringType = ((MethodDefinition)instruction.Operand).DeclaringType;
                                Console.WriteLine($"    !!!! Replacing {instruction.Operand} with {replacingMethod} from {replacingMethod.Module.Assembly.FullName}");
                                MethodReference? methodRef = method.Module.ImportReference(replacingMethod);
                                instruction.Operand = methodRef;
                                typesForRemoval.Add(declaringType);
                                typesNeedingReplacement.Add((declaringType, methodRef.DeclaringType));
                            }
                            else if (instruction.OpCode == OpCodes.Callvirt && instruction.Operand.ToString().Contains("BaseProject.Class1"))
                            {
                                var replacingMethod = FindMethodInAssembly(replaceAssembly, ((MethodDefinition)instruction.Operand).FullName);
                                Console.WriteLine($"    !!!! Replacing {instruction.Operand} with {replacingMethod} from {replacingMethod.Module.Assembly.FullName}");
                                var declaringType = ((MethodDefinition)instruction.Operand).DeclaringType;
                                instruction.Operand = method.Module.ImportReference(replacingMethod);
                            }
                            else
                            {
                                Console.WriteLine($"    op: {instruction.OpCode} {instruction.Operand}");
                            }


                        }

                        /* replace variables */
                        List<VariableDefinition> variablesToRemove = new();
                        List<VariableDefinition> variablesToAdd = new();
                        /* find variables that have to be replaced with types from the replacing assembly */
                        foreach (VariableDefinition? variable in il.Body.Variables)
                        {
                            Console.WriteLine("Variable: " + variable + " " + variable.VariableType + " " + variable.VariableType.Module.Assembly.FullName);
                            foreach (var typeThatNeedsReplacement in typesNeedingReplacement)
                            {
                                if (variable.VariableType == typeThatNeedsReplacement.oldRef)
                                {
                                    variablesToRemove.Add(variable);
                                    variablesToAdd.Add(new VariableDefinition(typeThatNeedsReplacement.newRef));
                                }
                            }
                        }

                        /* remove variables that need replacement */
                        foreach (var variable in variablesToRemove)
                        {
                            il.Body.Variables.Remove(variable);
                        }

                        /* add new variable with replacing type */
                        foreach (var variable in variablesToAdd)
                        {
                            il.Body.Variables.Add(variable);
                        }

                        Console.WriteLine("After Replace");

                        foreach (VariableDefinition? variable in il.Body.Variables)
                        {
                            Console.WriteLine("Variable: " + variable + " " + variable.VariableType + " " + variable.VariableType.Module.Assembly.FullName);
                        }

                        Console.WriteLine("}");
                    }
                }

                foreach (var type in typesForRemoval)
                {
                    module.Types.Remove(type);
                }

                foreach (var memberRef in module.GetMemberReferences())
                {
                    Console.WriteLine(" m: " + memberRef?.FullName);
                }
            }

            baseAssembly.Write(@"..\..\..\..\RunIt\bin\Debug\net6.0\BaseProject.dll");


        }

        static MethodDefinition FindMethodInAssembly(AssemblyDefinition assembly, string methodName)
        {
            foreach (var module in assembly.Modules)
            {
                foreach (var type in module.GetTypes())
                {
                    foreach (var method in type.Methods)
                    {
                        if (method.FullName == methodName)
                        {
                            return method;
                        }
                    }
                }
            }
            throw new KeyNotFoundException(String.Format("Method '{0}' not found.", methodName));
        }
    }
}
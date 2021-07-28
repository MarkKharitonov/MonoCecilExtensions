using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace MonoCecilExtensions
{
    public static class Extensions
    {
        private static int ProducedToStackBy(Instruction instruction) => instruction.OpCode.StackBehaviourPush switch
        {
            StackBehaviour.Push0 => 0,
            StackBehaviour.Push1_push1 => 2,
            StackBehaviour.Varpush => IsNonVoidMethodCall(instruction) ? 1 : 0,
            _ => 1,
        };

        private static bool IsNonVoidMethodCall(Instruction instruction) =>
            (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt) &&
            instruction.Operand is MethodReference method &&
            method.ReturnType.FullName != "System.Void";

        public static TypeReference Normalize(this TypeReference t)
        {
            bool cont;
            do
            {
                cont = false;
                switch (t)
                {
                    case ArrayType arrayType:
                        cont = true;
                        t = arrayType.ElementType;
                        break;
                    case ByReferenceType byReferenceType:
                        cont = true;
                        t = byReferenceType.ElementType;
                        break;
                }
            }
            while (cont);
            return t;
        }

        private static int ConsumedFromStackBy(Instruction instruction) => instruction.OpCode.StackBehaviourPop switch
        {
            StackBehaviour.Popi or
            StackBehaviour.Popref or
            StackBehaviour.Pop1 => 1,
            StackBehaviour.Pop1_pop1 or
            StackBehaviour.Popi_pop1 or
            StackBehaviour.Popi_popi or
            StackBehaviour.Popi_popi8 or
            StackBehaviour.Popi_popr4 or
            StackBehaviour.Popi_popr8 or
            StackBehaviour.Popref_pop1 or
            StackBehaviour.Popref_popi => 2,
            StackBehaviour.Popi_popi_popi or
            StackBehaviour.Popref_popi_popi or
            StackBehaviour.Popref_popi_popi8 or
            StackBehaviour.Popref_popi_popr4 or
            StackBehaviour.Popref_popi_popr8 or
            StackBehaviour.Popref_popi_popref => 3,
            StackBehaviour.Varpop => AnalyzeVariablePop(instruction),
            _ => 0,
        };

        private static int AnalyzeVariablePop(Instruction methodCall) =>
            (methodCall.HasThisObject() ? 1 : 0) + ((MethodReference)methodCall.Operand).Parameters.Count;

        private static Instruction MoveBackOnILStack(Instruction instruction, bool first, int consumedFromStack)
        {
            while (consumedFromStack > 0)
            {
                do
                {
                    instruction = instruction.Previous;
                }
                while (instruction.OpCode.OpCodeType == OpCodeType.Prefix);

                while (instruction.OpCode.FlowControl == FlowControl.Branch && instruction.Operand == instruction.Next || instruction.OpCode == OpCodes.Nop)
                {
                    instruction = instruction.Previous;
                }

                if (instruction.OpCode.FlowControl == FlowControl.Branch)
                {
                    var next = instruction.Next;
                    do
                    {
                        instruction = instruction.Previous;
                    }
                    while (instruction.Operand != next);
                    if (instruction.OpCode.FlowControl == FlowControl.Branch)
                    {
                        instruction = instruction.Previous;
                    }
                }

                --consumedFromStack;
                var prevProducedToStack = ProducedToStackBy(instruction);
                if (prevProducedToStack == 0 && instruction.OpCode.OpCodeType != OpCodeType.Prefix)
                {
                    ++consumedFromStack;
                }

                if (!first || consumedFromStack != 0 || instruction.Operand is not MemberReference)
                {
                    var prevConsumedFromStack = ConsumedFromStackBy(instruction);
                    if (prevProducedToStack > 1)
                    {
                        prevConsumedFromStack -= prevProducedToStack - 1;
                    }
                    instruction = MoveBackOnILStack(instruction, first && consumedFromStack == 0, prevConsumedFromStack);
                }
            }
            return instruction;
        }

        public static bool HasThisObject(this Instruction instruction) =>
            (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt) &&
            instruction.Operand is MethodReference methodRef &&
            methodRef.HasThis;

        public static TypeReference GetDeclaredTypeOfThisObject(this Instruction instruction, MethodDefinition parentMethodDef, IGenericContext gc = null)
        {
            var declared = parentMethodDef.DeclaringType;

            instruction = MoveBackOnILStack(instruction, true, ConsumedFromStackBy(instruction));
            while (instruction.OpCode == OpCodes.Dup)
            {
                instruction = MoveBackOnILStack(instruction, true, ConsumedFromStackBy(instruction));
            }

            if (instruction.OpCode == OpCodes.Ldloc_0 ||
                instruction.OpCode == OpCodes.Ldloc_1 ||
                instruction.OpCode == OpCodes.Ldloc_2 ||
                instruction.OpCode == OpCodes.Ldloc_3)
            {
                var localIndex = instruction.OpCode.Op2 - OpCodes.Ldloc_0.Op2;
                return ResolveGenericType(parentMethodDef.Body.Variables[localIndex].VariableType.Normalize(), null, gc);
            }
            if (instruction.OpCode == OpCodes.Ldarg_0 ||
                instruction.OpCode == OpCodes.Ldarg_1 ||
                instruction.OpCode == OpCodes.Ldarg_2 ||
                instruction.OpCode == OpCodes.Ldarg_3)
            {
                var localIndex = instruction.OpCode.Op2 - OpCodes.Ldarg_0.Op2;
                if (!parentMethodDef.IsStatic)
                {
                    if (localIndex == 0)
                    {
                        return declared;
                    }
                    --localIndex;
                }
                return ResolveGenericType(parentMethodDef.Parameters[localIndex].ParameterType.Normalize(), null, gc);
            }
            return instruction.Operand switch
            {
                MethodReference mr => ResolveGenericReturnType(mr, gc),
                TypeReference tr => ResolveGenericType(tr, tr.DeclaringType, gc),
                FieldReference fr => ResolveGenericType(fr.FieldType.Normalize(), fr.DeclaringType, gc),
                VariableReference vr => ResolveGenericType(vr.VariableType.Normalize(), null, gc),
                ParameterReference pr => ResolveGenericType(pr.ParameterType.Normalize(), null, gc),
                _ => null,
            };
        }

        private static TypeReference ResolveGenericReturnType(MethodReference mr, IGenericContext gc)
        {
            if (mr.ReturnType is not GenericParameter gp)
            {
                return mr.Name == ".ctor" ? mr.DeclaringType : mr.ReturnType;
            }

            if (mr is GenericInstanceMethod m)
            {
                var res = MatchGenericParameter(gp, m.ElementMethod.GenericParameters, m.GenericArguments);
                if (res != null)
                {
                    return res;
                }
            }

            return ResolveGenericType(mr.ReturnType, mr.DeclaringType, gc);
        }

        private static TypeReference ResolveGenericType(TypeReference tr, TypeReference declaringType, IGenericContext gc)
        {
            if (tr is not GenericParameter gp)
            {
                return tr;
            }

            if (declaringType is GenericInstanceType g)
            {
                var res = MatchGenericParameter(gp, g.ElementType.GenericParameters, g.GenericArguments);
                if (res != null)
                {
                    return res;
                }
            }

            return gc.Resolve(gp);
        }

        private static TypeReference MatchGenericParameter(GenericParameter gp, IList<GenericParameter> genericParameters, IList<TypeReference> genericArguments) =>
            genericParameters.Count > gp.Position && genericParameters[gp.Position] == gp ?
            genericArguments[gp.Position] :
            null;
    }
}
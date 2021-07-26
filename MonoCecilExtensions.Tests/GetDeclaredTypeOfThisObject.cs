using Mono.Cecil;
using Mono.Cecil.Cil;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MonoCecilExtensions.Tests
{
    public class GetDeclaredTypeOfThisObject
    {
        public interface IBase
        {
            void Action0();
            void Action1(int? x);
            int Func0();
            int Func1(object x);
            int Func2(object x, object y);
        }

        public interface IDerived : IBase
        {
        }

        public class Derived : IDerived
        {
            public void Action0() => throw new NotImplementedException();

            public void Action1(int? x) => throw new NotImplementedException();

            public int Func0() => throw new NotImplementedException();

            public int Func1(object x) => throw new NotImplementedException();

            public int Func2(object x, object y) => throw new NotImplementedException();
        }

        private static IDerived GetObject() => new Derived();
        private static IDerived GetObject(int _) => new Derived();
        private IDerived GetObject2() => new Derived();
        private IDerived GetObject2(int _) => new Derived();

        private static int s_x = 5;
        private int m_x = 5;
        private readonly static IDerived s_o = GetObject();
        private readonly IDerived m_o = GetObject();

        public static void Func0_Direct()
        {
            var o = GetObject();
            o.Func0();
        }

        public static void Func0_DirectAsCast()
        {
            IBase o = GetObject();
            ((IDerived)o).Func0();
        }

        public static void Func0_DirectAsCast2()
        {
            IBase o = GetObject();
            (o as IDerived).Func0();
        }

        public static void Func0_DirectAsArrayElement()
        {
            var a = new[] { GetObject() };
            a[0].Func0();
        }

        public static void Func0_DirectAsStaticMember()
        {
            s_o.Func0();
        }

        public void Func0_DirectAsInstanceMember()
        {
            m_o.Func0();
        }

        public static void Func1_DirectAsStaticMemberWithConstArg()
        {
            s_o.Func1(1000);
        }

        public void Func1_DirectAsInstanceMemberWithConstArg()
        {
            m_o.Func1(1000);
        }

        public static void Func1_DirectAsStaticMemberWithArgAsFuncResult()
        {
            s_o.Func1(new Random().Next(1));
        }

        public void Func1_DirectAsInstanceMemberWithArgAsAnonymousArrayItem()
        {
            m_o.Func1(new[] { 10 }[0]);
        }

        public static void Func0_Indirect()
        {
            GetObject().Func0();
        }

        public void Func0_Indirect2()
        {
            GetObject2().Func0();
        }

        public static void Func0_IndirectWithArg()
        {
            GetObject(0).Func0();
        }

        public void Func0_IndirectWithArg2()
        {
            GetObject2(0).Func0();
        }

        public static void Action1_DirectWithFuncResult()
        {
            var o = GetObject();
            o.Action1(o.Func0());
        }

        public static void Action1_DirectWithFuncResult2()
        {
            var o = GetObject();
            o.Action1(GetObject().Func1((int)Math.PI));
        }

        public static void Func1_DirectWithConstArg()
        {
            var o = GetObject();
            o.Func1(5);
        }

        public static void Func1_DirectWithCast()
        {
            var o = GetObject();
            o.Func1((int)Math.PI);
        }

        public static void Func1_DirectWithArgFromParams(int x)
        {
            var o = GetObject();
            o.Func1(x);
        }

        public static void Func1_DirectWithArgFromStaticMembers()
        {
            var o = GetObject();
            o.Func1(s_x);
        }

        public void Func1_DirectWithArgFromInstanceMembers()
        {
            var o = GetObject();
            o.Func1(m_x);
        }

        private static T Identity<T>(T o) => o;

        private static class IdentityObj<T>
        {
            public static T Value(T o) => o;
        }

        public static void Func0_IndirectAsGenericFuncResult()
        {
            Identity(s_o).Func0();
        }

        public static void Func0_IndirectAsGenericTypeMethodResult()
        {
            new List<IDerived> { s_o }[0].Func0();
        }
        public static void Func0_IndirectAsGenericTypeMethodResult2()
        {
            IdentityObj<IDerived>.Value(s_o).Func0();
        }

        public void Func2_MixedAndComplexWithConstArgs()
        {
            new List<IDerived>
            {
                GetObject2(new[]{ 1000 }[0])
            }[0].Func2(0, null);
        }

        public void Func2_MixedAndComplex()
        {
            new List<IDerived>
            {
                GetObject2(new[]{ 1000 }[0])
            }[0].Func2(Identity(int.Parse("0")), IdentityObj<string>.Value(("", 0).Item1));
        }

        private class GenericClass<TIntf, TImpl>
            where TIntf : IBase
            where TImpl : class, TIntf, new()
        {
            public readonly TIntf m_o;

            public GenericClass(object x) : this()
            {
            }

            public GenericClass()
            {
                m_o = new TImpl();
            }

            public TIntf Value => m_o;
            public TIntf GetValue() => m_o;

            public void Func0_Direct()
            {
                var o = GetValue();
                Assert.IsNotNull(o);
                o.Func0();
            }

            public void Func0_DirectAsField() => m_o.Func0();

            public void Func0_DirectAsProperty() => Value.Func0();

            public void Func0_Indirect() => GetValue().Func0();

            public TIntf this[int _] => m_o;
            public static explicit operator TIntf(GenericClass<TIntf, TImpl> o) => o.m_o;
        }

        public void Func0_DirectAsGenericClassField()
        {
            new GenericClass<IDerived, Derived>(5).m_o.Func0();
        }

        public void Func0_DirectAsGenericClassProperty()
        {
            new GenericClass<IDerived, Derived>("xyz").Value.Func0();
        }

        public void Func0_IndirectAsGenericClassMethod()
        {
            new GenericClass<IDerived, Derived>(new decimal(3.45)).GetValue().Func0();
        }

        public void Func0_IndirectAsGenericClassIndexer()
        {
            new GenericClass<IDerived, Derived>()[100].Func0();
        }

        public void Func0_IndirectAsGenericClassExplicitCast()
        {
            ((IDerived)new GenericClass<IDerived, Derived>(new StringBuilder("hello" + " world"))).Func0();
        }

        public void Func2_DirectAsGenericClassFieldMixedArgs()
        {
            new GenericClass<IDerived, Derived>().m_o.Func2(new[] { DateTime.Now }[(int)double.Parse(Math.PI.ToString()) - 3], string.Join(',', 1, 2, 3));
        }

        public void Func1_DirectMixedArgs6()
        {
            m_o.Func1(new[] { DateTime.Now }[(int)double.Parse(Math.PI.ToString()) - 3]);
        }

        public void Func1_DirectMixedArgs5()
        {
            m_o.Func1((int)double.Parse(Math.PI.ToString()) - 3);
        }

        public void Func1_DirectMixedArgs4()
        {
            m_o.Func1((int)double.Parse(3.1.ToString()) - 3);
        }

        public void Func1_DirectMixedArgs3()
        {
            m_o.Func1(3.ToString());
        }

        public void Func1_DirectMixedArgs2()
        {
            m_o.Func1((int)double.Parse("3.1") - 3);
        }

        public void Func1_DirectMixedArgs()
        {
            m_o.Func1((int)3.4 - 3);
        }

        public void Func1_DirectWithArgTernaryOp2()
        {
            m_o.Func1(DateTime.Now > DateTime.Parse("2021-07-23") ? "hello" : "world");
        }
#pragma warning disable 649
        private static bool s_bool;
        private static bool s_bool2;
        private static bool s_bool3;
        private static bool s_bool4;
        private static string s_str;
#pragma warning restore 649

        public void Func1_DirectWithArgTernaryOp()
        {
            m_o.Func1(s_bool ? "hello" : "world");
        }

        public void Func1_DirectWithArgNestedTernaryOp()
        {
            m_o.Func1(s_bool ? s_bool2 ? "hello" : "world" : "bye");
        }

        public void Func1_DirectWithArgNestedTernaryOp2()
        {
            m_o.Func1(s_bool ? s_bool2 ? "world" : s_bool3 ? "hello" : "bye" : "good");
        }

        public void Action1_DirectWithArgNestedTernaryOp3()
        {
            m_o.Action1(s_bool4 ? null : s_str?.Length);
        }

        public void Func1_DirectWithArgNestedTernaryOp4()
        {
            m_o.Func1(s_bool4 ? "1" : s_str?.Length);
        }

        public void Func1_DirectWithArgNestedTernaryOp5()
        {
            m_o.Func1(s_bool3 ? "hello" : s_bool4 ? "1" : s_str?.Length);
        }

        public void Func1_DirectWithArgNullCoalesceOp()
        {
            m_o.Func1(s_str ?? "world");
        }

        public void Func1_DirectWithArgNullCoalesceAssignOp()
        {
            string x = s_str;
            m_o.Func1(x ??= "world");
        }

        public void Func1_DirectWithArgNullPropagateOp()
        {
            string x = s_str;
            m_o.Func1(x?.Length);
        }

        public void Action1_DirectWithArgNullPropagateOp()
        {
            string x = s_str;
            m_o.Action1(x?.Length);
        }

        public void Action1_DirectWithArgNull()
        {
            m_o.Action1(null);
        }

        public void Func2_DirectMixedArgs()
        {
            m_o.Func2(new[] { DateTime.Now }[(int)double.Parse(Math.PI.ToString()) - 3], string.Join(',', 1, 2, 3));
        }

        public void Func2_DirectAsGenericClassPropertyMixedArgs()
        {
            new GenericClass<IDerived, Derived>("xyz").Value.Func2(new[] { DateTime.Now }[(int)double.Parse(Math.PI.ToString()) - 3], string.Join(',', 1, 2, 3));
        }

        public void Func2_IndirectAsGenericClassMethodMixedArgs()
        {
            new GenericClass<IDerived, Derived>(new decimal(3.45)).GetValue().Func2(new[] { DateTime.Now }[(int)double.Parse(Math.PI.ToString()) - 3], string.Join(',', 1, 2, 3));
        }

        public void Func2_IndirectAsGenericClassIndexerMixedArgs()
        {
            new GenericClass<IDerived, Derived>()[122].Func2(new[] { DateTime.Now }[(int)double.Parse(Math.PI.ToString()) - 3], string.Join(',', 1, 2, 3));
        }

        public void Func2_IndirectAsGenericClassExplicitCastMixedArgs()
        {
            ((IDerived)new GenericClass<IDerived, Derived>(DateTime.Now)).Func2(new StringBuilder("hello" + " world"), Enumerable.Range(0, 10).ToArray()[8]);
        }

        private (AssemblyDefinition, MethodDefinition, Instruction) GetContext(string methodName, string typeName = null)
        {
            var asmDef = AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location);
            var targetName = methodName.Substring(0, methodName.IndexOf('_'));
            MethodDefinition methodDef = null;
            var typeDef = asmDef.MainModule.GetType(typeName ?? GetType().FullName);
            methodDef = typeDef.Methods.SingleOrDefault(m => m.Name == methodName);
            Assert.IsNotNull(methodDef, "Not found or ambiguous match for " + methodName);
            var instruction = methodDef.Body.Instructions.FirstOrDefault(i => i.Operand is MethodReference mr && mr.Name == targetName);
            Assert.IsNotNull(instruction, $"Not found call to {targetName} in {methodName}");
            var methodRef = (MethodReference)instruction.Operand;
            Assert.AreEqual(nameof(IBase), methodRef.DeclaringType.Name);
            return (asmDef, methodDef, instruction);
        }

        [TestCase(nameof(Func0_Direct))]
        [TestCase(nameof(Func0_DirectAsCast))]
        [TestCase(nameof(Func0_DirectAsCast2))]
        [TestCase(nameof(Func0_DirectAsArrayElement))]
        [TestCase(nameof(Func0_Indirect))]
        [TestCase(nameof(Func0_Indirect2))]
        [TestCase(nameof(Func0_IndirectWithArg))]
        [TestCase(nameof(Func0_IndirectWithArg2))]
        [TestCase(nameof(Func0_DirectAsGenericClassField))]
        [TestCase(nameof(Func0_DirectAsGenericClassProperty))]
        [TestCase(nameof(Func0_IndirectAsGenericClassMethod))]
        [TestCase(nameof(Func0_IndirectAsGenericClassIndexer))]
        [TestCase(nameof(Func0_IndirectAsGenericClassExplicitCast))]
        [TestCase(nameof(Func1_DirectWithArgNestedTernaryOp5))]
        [TestCase(nameof(Func1_DirectWithArgNestedTernaryOp4))]
        [TestCase(nameof(Action1_DirectWithArgNestedTernaryOp3))]
        [TestCase(nameof(Func1_DirectWithArgNestedTernaryOp2))]
        [TestCase(nameof(Func1_DirectWithArgNestedTernaryOp))]
        [TestCase(nameof(Func1_DirectWithArgTernaryOp))]
        [TestCase(nameof(Func1_DirectWithArgTernaryOp2))]
        [TestCase(nameof(Func1_DirectWithArgNullCoalesceOp))]
        [TestCase(nameof(Action1_DirectWithArgNull))]
        [TestCase(nameof(Action1_DirectWithArgNullPropagateOp))]
        [TestCase(nameof(Func1_DirectWithArgNullPropagateOp))]
        [TestCase(nameof(Func1_DirectWithArgNullCoalesceAssignOp))]
        [TestCase(nameof(Func1_DirectMixedArgs))]
        [TestCase(nameof(Func1_DirectMixedArgs2))]
        [TestCase(nameof(Func1_DirectMixedArgs3))]
        [TestCase(nameof(Func1_DirectMixedArgs4))]
        [TestCase(nameof(Func1_DirectMixedArgs5))]
        [TestCase(nameof(Func1_DirectMixedArgs6))]
        [TestCase(nameof(Func2_DirectMixedArgs))]
        [TestCase(nameof(Func2_DirectAsGenericClassFieldMixedArgs))]
        [TestCase(nameof(Func2_DirectAsGenericClassPropertyMixedArgs))]
        [TestCase(nameof(Func2_IndirectAsGenericClassMethodMixedArgs))]
        [TestCase(nameof(Func2_IndirectAsGenericClassIndexerMixedArgs))]
        [TestCase(nameof(Func2_IndirectAsGenericClassExplicitCastMixedArgs))]
        [TestCase(nameof(Action1_DirectWithFuncResult))]
        [TestCase(nameof(Action1_DirectWithFuncResult2))]
        [TestCase(nameof(Func1_DirectWithConstArg))]
        [TestCase(nameof(Func1_DirectWithCast))]
        [TestCase(nameof(Func1_DirectWithArgFromParams))]
        [TestCase(nameof(Func1_DirectWithArgFromStaticMembers))]
        [TestCase(nameof(Func1_DirectWithArgFromInstanceMembers))]
        [TestCase(nameof(Func1_DirectAsStaticMemberWithConstArg))]
        [TestCase(nameof(Func1_DirectAsInstanceMemberWithConstArg))]
        [TestCase(nameof(Func1_DirectAsStaticMemberWithArgAsFuncResult))]
        [TestCase(nameof(Func1_DirectAsInstanceMemberWithArgAsAnonymousArrayItem))]
        [TestCase(nameof(Func0_DirectAsStaticMember))]
        [TestCase(nameof(Func0_DirectAsInstanceMember))]
        [TestCase(nameof(Func0_IndirectAsGenericTypeMethodResult))]
        [TestCase(nameof(Func0_IndirectAsGenericTypeMethodResult2))]
        [TestCase(nameof(Func0_IndirectAsGenericFuncResult))]
        [TestCase(nameof(Func2_MixedAndComplexWithConstArgs))]
        [TestCase(nameof(Func2_MixedAndComplex))]
        public void Test(string methodName)
        {
            var (asmDef, methodDef, instruction) = GetContext(methodName);
            using (asmDef)
            {
                var typeRef = instruction.GetDeclaredTypeOfThisObject(methodDef.Body.Variables, new Mock<IGenericContext>().Object);
                Assert.IsNotNull(typeRef);
                Assert.AreEqual(nameof(IDerived), typeRef.Name);
            }
        }

        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_Direct))]
        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_DirectAsField))]
        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_DirectAsProperty))]
        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_Indirect))]
        public void NestedInGeneric(string methodName) => DoTestGeneric(methodName);

        private void DoTestGeneric(string methodName)
        {
            var (asmDef, methodDef, instruction) = GetContext(methodName, typeof(GenericClass<,>).FullName.Replace('+', '/'));
            using (asmDef)
            {
                var genericContextMock = new Mock<IGenericContext>();
                genericContextMock.Setup(o => o.Resolve(It.Is<GenericParameter>(gp => gp.Name == "TIntf"))).Returns(asmDef.MainModule.ImportReference(typeof(IDerived)));
                var typeRef = instruction.GetDeclaredTypeOfThisObject(methodDef.Body.Variables, genericContextMock.Object);
                Assert.AreEqual(nameof(IDerived), typeRef.Name);
            }
        }
    }
}

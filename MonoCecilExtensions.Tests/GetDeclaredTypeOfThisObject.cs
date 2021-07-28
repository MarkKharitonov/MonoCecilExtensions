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
            void Action2(int x);
            void Action3(object x);
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

            public void Action0_AsMemberOfThisClass()
            {
                Action0();
            }

            public static void Action0_AsStaticMemberOfThisClass(Derived o)
            {
                o.Action0();
            }

            public void Action2(int x) => throw new NotImplementedException();

            public void Action3(object x) => throw new NotImplementedException();
        }

        public class Derived2 : Derived
        {
            public void Action0_AsMemberOfBaseClass()
            {
                Action0();
            }
        }

        private static IDerived GetObject() => new Derived();
        private static IDerived GetObject(int _) => new Derived();
        private IDerived GetObject2() => new Derived();
        private IDerived GetObject2(int _) => new Derived();

        private static int s_x = 5;
        private int m_x = 5;
        private readonly static IDerived s_o = GetObject();
        private readonly IDerived m_o = GetObject();
        private readonly IDerived[] m_a = new IDerived[10];

        public static void Func0_Direct()
        {
            var o = GetObject();
            o.Func0();
        }

        public void Func0_Direct2()
        {
            var x = DateTime.Now.Ticks;
            Assert.AreNotEqual(0, x);
            var o = GetObject();
            Assert.IsNotNull(o);
            o.Func0();
        }

        public void Func0_Direct3()
        {
            var x = DateTime.Now.Ticks;
            Assert.AreNotEqual(0, x);
            var y = DateTime.Now.Ticks;
            Assert.AreNotEqual(0, y);
            var o = GetObject();
            Assert.IsNotNull(o);
            o.Func0();
        }

        public void Func0_Direct4()
        {
            var x = DateTime.Now.Ticks;
            Assert.AreNotEqual(0, x);
            var y = DateTime.Now.Ticks;
            Assert.AreNotEqual(0, y);
            var z = DateTime.Now.Ticks;
            Assert.AreNotEqual(0, z);
            var o = GetObject();
            Assert.IsNotNull(o);
            o.Func0();
        }

        public void Func0_DirectAsArg(IDerived x) => x.Func0();
        public void Func0_DirectAsArg2(int a, IDerived x) => x.Func0();
        public void Func0_DirectAsArg3(int a, int b, IDerived x) => x.Func0();
        public void Func0_DirectAsArg4(int a, int b, int c, IDerived x) => x.Func0();
        public void Func0_DirectAsArg5FromParams(int a, int b, int c, int d, params IDerived[] x) => x[0].Func0();
        public static void Func0_StaticDirectAsArg(IDerived x) => x.Func0();
        public static void Func0_StaticDirectAsArg2(int a, IDerived x) => x.Func0();
        public static void Func0_StaticDirectAsArg3(int a, int b, IDerived x) => x.Func0();
        public static void Func0_StaticDirectAsArg4(int a, int b, int c, IDerived x) => x.Func0();
        public static void Func0_StaticDirectAsArg5FromParams(int a, int b, int c, int d, params IDerived[] x) => x[0].Func0();

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

        public void Func0_DirectAsInstanceArrayMemberItem()
        {
            m_a[0].Func0();
        }

        public static void Func1_DirectAsStaticMemberWithConstArg()
        {
            s_o.Func1(1000);
        }

        public void Func1_DirectAsInstanceMemberWithConstArgBoxed()
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

        public static void Action1_DirectWithFuncResultConvertedToNullable()
        {
            var o = GetObject();
            o.Action1(o.Func0());
        }

        public static void Action1_DirectWithConstArgConvertedToNullable()
        {
            var o = GetObject();
            o.Action1(5);
        }

        public static void Action2_DirectWithFuncResultNoConversion()
        {
            var o = GetObject();
            o.Action2(o.Func0());
        }

        public static void Action3_DirectWithFuncResultAsBoxed()
        {
            var o = GetObject();
            o.Action3(o.Func0());
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

            public void Func0_DirectAsArgIntf(TIntf x) => x.Func0();
            public void Func0_DirectAsArgImpl(TImpl x) => x.Func0();

            public void Func0_DirectAsArg2(int a, TIntf x) => x.Func0();
            public void Func0_DirectAsArg3(int a, int b, TIntf x) => x.Func0();
            public void Func0_DirectAsArg4(int a, int c, int d, TIntf x) => x.Func0();
            public void Func0_DirectAsArg5(int a, int c, int d, int e, TIntf x) => x.Func0();

            public static void Func0_StaticDirectAsArg(TIntf x) => x.Func0();
            public static void Func0_StaticDirectAsArg2(int a, TIntf x) => x.Func0();
            public static void Func0_StaticDirectAsArg3(int a, int b, TIntf x) => x.Func0();
            public static void Func0_StaticDirectAsArg4(int a, int c, int d, TIntf x) => x.Func0();
            public static void Func0_StaticDirectAsArg5(int a, int c, int d, int e, TIntf x) => x.Func0();

            public void Func0_Indirect() => GetValue().Func0();

            public TIntf this[int _] => m_o;
            public static explicit operator TIntf(GenericClass<TIntf, TImpl> o) => o.m_o;
        }

        private class GenericDerived<TIntf, TImpl> : Derived
            where TIntf : IBase
            where TImpl : class, TIntf, new()
        {
            public void Func0_DirectAsMemberOfBaseClass() => Func0();
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

        public void Func0_IndirectAsInheritedMethod()
        {
            new Derived2().Func0();
        }

        public void Func0_IndirectAsExplicitCastToInterface()
        {
            ((IDerived)new Derived()).Func0();
        }

        public void Func0_IndirectAsGenericDerivedExplicitCast()
        {
            ((IDerived)new GenericDerived<IDerived, Derived>()).Func0();
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

        private (AssemblyDefinition, MethodDefinition, Instruction) GetContext(string methodName, Type type = null, bool assertDeclaredType = true)
        {
            var targetName = methodName.Substring(0, methodName.IndexOf('_'));
            return GetContext(methodName, targetName, type, assertDeclaredType);
        }

        private (AssemblyDefinition, MethodDefinition, Instruction) GetContext(string methodName, string targetName, Type type, bool assertDeclaredType,
            int index = 0)
        {
            var asmDef = AssemblyDefinition.ReadAssembly(Assembly.GetExecutingAssembly().Location);
            var typeDef = asmDef.MainModule.ImportReference(type ?? GetType()).Resolve();
            var methodDefs = typeDef.Methods.Where(m => m.Name == methodName).ToList();
            Assert.AreEqual(1, methodDefs.Count, $"Found {methodDefs.Count} methods named {methodName}");
            var instructions = methodDefs[0].Body.Instructions.Where(i => i.Operand is MethodReference mr && mr.Name == targetName).ToList();
            Assert.IsNotEmpty(instructions, $"Not found call to {targetName} in {methodName}");
            if (assertDeclaredType)
            {
                var methodRef = (MethodReference)instructions[index].Operand;
                Assert.AreEqual(nameof(IBase), methodRef.DeclaringType.Name);
            }
            return (asmDef, methodDefs[0], instructions[index]);
        }

        [TestCase(nameof(Func0_Direct))]
        [TestCase(nameof(Func0_Direct2))]
        [TestCase(nameof(Func0_Direct3))]
        [TestCase(nameof(Func0_Direct4))]
        [TestCase(nameof(Func0_DirectAsCast))]
        [TestCase(nameof(Func0_DirectAsCast2))]
        [TestCase(nameof(Func0_DirectAsArrayElement))]
        [TestCase(nameof(Func0_DirectAsArg))]
        [TestCase(nameof(Func0_DirectAsArg2))]
        [TestCase(nameof(Func0_DirectAsArg3))]
        [TestCase(nameof(Func0_DirectAsArg4))]
        [TestCase(nameof(Func0_DirectAsArg5FromParams))]
        [TestCase(nameof(Func0_StaticDirectAsArg))]
        [TestCase(nameof(Func0_StaticDirectAsArg2))]
        [TestCase(nameof(Func0_StaticDirectAsArg3))]
        [TestCase(nameof(Func0_StaticDirectAsArg4))]
        [TestCase(nameof(Func0_StaticDirectAsArg5FromParams))]
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
        [TestCase(nameof(Action1_DirectWithFuncResultConvertedToNullable))]
        [TestCase(nameof(Action1_DirectWithConstArgConvertedToNullable))]
        [TestCase(nameof(Action2_DirectWithFuncResultNoConversion))]
        [TestCase(nameof(Action3_DirectWithFuncResultAsBoxed))]
        [TestCase(nameof(Action1_DirectWithFuncResult2))]
        [TestCase(nameof(Func1_DirectWithConstArg))]
        [TestCase(nameof(Func1_DirectWithCast))]
        [TestCase(nameof(Func1_DirectWithArgFromParams))]
        [TestCase(nameof(Func1_DirectWithArgFromStaticMembers))]
        [TestCase(nameof(Func1_DirectWithArgFromInstanceMembers))]
        [TestCase(nameof(Func1_DirectAsStaticMemberWithConstArg))]
        [TestCase(nameof(Func1_DirectAsInstanceMemberWithConstArgBoxed))]
        [TestCase(nameof(Func1_DirectAsStaticMemberWithArgAsFuncResult))]
        [TestCase(nameof(Func1_DirectAsInstanceMemberWithArgAsAnonymousArrayItem))]
        [TestCase(nameof(Func0_DirectAsStaticMember))]
        [TestCase(nameof(Func0_DirectAsInstanceMember))]
        [TestCase(nameof(Func0_DirectAsInstanceArrayMemberItem))]
        [TestCase(nameof(Func0_IndirectAsGenericTypeMethodResult))]
        [TestCase(nameof(Func0_IndirectAsGenericTypeMethodResult2))]
        [TestCase(nameof(Func0_IndirectAsGenericFuncResult))]
        [TestCase(nameof(Func2_MixedAndComplexWithConstArgs))]
        [TestCase(nameof(Func2_MixedAndComplex))]
        public void Test(string methodName)
        {
            DoTest(methodName, typeof(IDerived));
        }

        [TestCase(nameof(Derived.Action0_AsMemberOfThisClass), null, typeof(Derived))]
        [TestCase(nameof(Derived.Action0_AsStaticMemberOfThisClass), null, typeof(Derived))]
        [TestCase(nameof(Derived2.Action0_AsMemberOfBaseClass), null, typeof(Derived2))]
        [TestCase(nameof(GenericDerived<IDerived, Derived>.Func0_DirectAsMemberOfBaseClass), null, typeof(GenericDerived<IDerived, Derived>))]
        [TestCase(nameof(Func0_IndirectAsExplicitCastToInterface), typeof(Derived), null)]
        public void DerivedParent(string methodName, Type expectedResult, Type type)
        {
            DoTest(methodName, expectedResult ?? type, type, false);
        }

        [TestCase(nameof(Func0_IndirectAsGenericDerivedExplicitCast), typeof(GenericDerived<IDerived, Derived>))]
        [TestCase(nameof(Func0_IndirectAsInheritedMethod), typeof(Derived2))]
        public void DerivedParent2(string methodName, Type type)
        {
            DoTest(methodName, type, null, false);
        }

        private void DoTest(string methodName, Type expectedResult, Type type = null, bool assertDeclaredType = true)
        {
            var (asmDef, methodDef, instruction) = GetContext(methodName, type, assertDeclaredType);
            using (asmDef)
            {
                var typeRef = instruction.GetDeclaredTypeOfThisObject(methodDef, new Mock<IGenericContext>().Object);
                Assert.IsNotNull(typeRef);
                Assert.AreEqual(expectedResult.Name, typeRef.Name);
            }
        }

        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_Direct))]
        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_DirectAsArgIntf))]
        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_DirectAsArgImpl))]
        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_DirectAsArg2))]
        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_DirectAsArg3))]
        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_DirectAsArg4))]
        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_DirectAsArg5))]
        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_StaticDirectAsArg))]
        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_StaticDirectAsArg2))]
        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_StaticDirectAsArg3))]
        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_StaticDirectAsArg4))]
        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_StaticDirectAsArg5))]
        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_DirectAsField))]
        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_DirectAsProperty))]
        [TestCase(nameof(GenericClass<IDerived, Derived>.Func0_Indirect))]
        public void GenericParent(string methodName) => DoTestGeneric(methodName);

        private void DoTestGeneric(string methodName)
        {
            var (asmDef, methodDef, instruction) = GetContext(methodName, typeof(GenericClass<,>));
            using (asmDef)
            {
                var genericContextMock = new Mock<IGenericContext>();
                genericContextMock
                    .Setup(o => o.Resolve(It.Is<GenericParameter>(gp => gp.Name == "TIntf" || gp.Name == "TImpl")))
                    .Returns(asmDef.MainModule.ImportReference(typeof(IDerived)))
                    .Verifiable();
                var typeRef = instruction.GetDeclaredTypeOfThisObject(methodDef, genericContextMock.Object);
                genericContextMock.Verify(o => o.Resolve(It.Is<GenericParameter>(gp => gp.Name == "TIntf" || gp.Name == "TImpl")), Times.Exactly(1));
                Assert.AreEqual(nameof(IDerived), typeRef.Name);
            }
        }

        private class DTO
        {
            public string Field1 { get; set; }
            public bool Field2 { get; set; }
            public int x { get; set; }
        }

        private List<DTO> WithDup()
        {
            return new List<DTO>
            {
                new DTO
                {
                    Field1 = "Hello",
                    Field2 = true
                }
            };
        }

        private void TernaryWithAndAlwaysTrue(int? y)
        {
            var o = new DTO();
#pragma warning disable CS0472
            o.x = (s_bool && y.Value != null) ? 5 : 0;
#pragma warning restore CS0472
        }

        private void SomeFunction(Func<int> f) => throw new NotImplementedException();

        private int CapturedWithArray()
        {
            var a = new DTO[10];
            SomeFunction(() => a[0].x);
            return a[0].x;
        }

        private void NamedParamWithDefaults()
        {
            SomeFunction2(y: true);
        }

        private void SomeFunction2(int? x = null, bool? y = null)
        {
            throw new NotImplementedException();
        }

        [TestCase(nameof(WithDup), "set_" + nameof(DTO.Field1), typeof(DTO), 0)]
        [TestCase(nameof(WithDup), "set_" + nameof(DTO.Field2), typeof(DTO), 0)]
        [TestCase(nameof(WithDup), nameof(List<DTO>.Add), typeof(List<DTO>), 0)]
        [TestCase(nameof(TernaryWithAndAlwaysTrue), "set_" + nameof(DTO.x), typeof(DTO), 0)]
        [TestCase(nameof(CapturedWithArray), "get_" + nameof(DTO.x), typeof(DTO), 0)]
        [TestCase(nameof(NamedParamWithDefaults), nameof(SomeFunction2), null, 0)]
        public void Misc(string methodName, string targetName, Type expectedType, int index)
        {
            var (asmDef, methodDef, instruction) = GetContext(methodName, targetName, null, false, index);
            using (asmDef)
            {
                var typeRef = instruction.GetDeclaredTypeOfThisObject(methodDef);
                Assert.AreEqual(asmDef.MainModule.ImportReference(expectedType ?? GetType()).FullName, typeRef.FullName);
            }
        }
    }
}

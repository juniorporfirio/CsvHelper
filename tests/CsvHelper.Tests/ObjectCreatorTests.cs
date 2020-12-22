using CsvHelper.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CsvHelper.Tests.ObjectCreatorTests
{
	[TestClass]
	public class CreateInstance_ValueType
	{
		[TestMethod]
		public void CreatesInstance()
		{
			var creator = new ObjectCreator();
			var value = creator.CreateInstance<int>();

			Assert.AreEqual(default(int), value);
		}

		[TestMethod]
		public void ParameterSupplied_ThrowsMissingMethodExcepetion()
		{
			var creator = new ObjectCreator();

			Assert.ThrowsException<MissingMethodException>(() => creator.CreateInstance<int>(1));
		}
	}

	[TestClass]
	public class CreateInstance_DefaultConstructor
	{
		[TestMethod]
		public void CreatesInstance()
		{
			var creator = new ObjectCreator();
			var foo = creator.CreateInstance<Foo>();
			creator.CreateInstance<Foo>();

			Assert.IsInstanceOfType(foo, typeof(Foo));
			Assert.AreEqual(default(int), foo.Id);
		}

		[TestMethod]
		public void ParameterSupplied_ThrowsMissingMethodExcepetion()
		{
			var creator = new ObjectCreator();

			Assert.ThrowsException<MissingMethodException>(() => creator.CreateInstance<Foo>(1));
		}

		private class Foo
		{
			public int Id { get; private set; }

			public Foo() { }
		}
	}

	[TestClass]
	public class CreateInstance_OneParameterConstructor
	{
		[TestMethod]
		public void OneParameter_CreatesInstance()
		{
			var creator = new ObjectCreator();
			var foo = creator.CreateInstance<Foo>(1);

			Assert.IsInstanceOfType(foo, typeof(Foo));
			Assert.AreEqual(1, foo.Id);
		}

		[TestMethod]
		public void NoParameter_ThrowsMissingMethodException()
		{
			var creator = new ObjectCreator();

			Assert.ThrowsException<MissingMethodException>(() => creator.CreateInstance<Foo>());
		}

		private class Foo
		{
			public int Id { get; private set; }

			public Foo(int id)
			{
				Id = id;
			}
		}
	}

	[TestClass]
	public class CreateInstance_DefaultConstructorAndOneParameterConstructor
	{
		[TestMethod]
		public void NoParameter_CreatesInstance()
		{
			var creator = new ObjectCreator();
			var foo = creator.CreateInstance<Foo>();

			Assert.IsInstanceOfType(foo, typeof(Foo));
			Assert.AreEqual(default(int), foo.Id);
		}

		[TestMethod]
		public void OneParameter_CreatesInstance()
		{
			var creator = new ObjectCreator();
			var foo = creator.CreateInstance<Foo>(1);

			Assert.IsInstanceOfType(foo, typeof(Foo));
			Assert.AreEqual(1, foo.Id);
		}

		[TestMethod]
		public void OneParameterWrongType_ThrowsMissingMethodException()
		{
			var creator = new ObjectCreator();

			Assert.ThrowsException<MissingMethodException>(() => creator.CreateInstance<Foo>(string.Empty));
		}

		private class Foo
		{
			public int Id { get; private set; }

			public Foo() { }

			public Foo(int id)
			{
				Id = id;
			}
		}
	}

	[TestClass]
	public class CreateInstance_ValueTypeAndReferenceTypeParameters
	{
		[TestMethod]
		public void FirstSignature_CreatesInstance()
		{
			var creator = new ObjectCreator();
			var foo = creator.CreateInstance<Foo>(1, "one");

			Assert.IsInstanceOfType(foo, typeof(Foo));
			Assert.AreEqual(1, foo.Id);
			Assert.AreEqual("one", foo.Name);
		}

		[TestMethod]
		public void SecondSignature_CreatesInstance()
		{
			var creator = new ObjectCreator();
			var foo = creator.CreateInstance<Foo>("one", 1);

			Assert.IsInstanceOfType(foo, typeof(Foo));
			Assert.AreEqual(1, foo.Id);
			Assert.AreEqual("one", foo.Name);
		}

		[TestMethod]
		public void FirstSignature_NullReferenceType_CreatesInstance()
		{
			var creator = new ObjectCreator();
			var foo = creator.CreateInstance<Foo>(1, null);

			Assert.IsInstanceOfType(foo, typeof(Foo));
			Assert.AreEqual(1, foo.Id);
			Assert.IsNull(foo.Name);
		}

		[TestMethod]
		public void SecondSignature_NullReferenceType_CreatesInstance()
		{
			var creator = new ObjectCreator();
			var foo = creator.CreateInstance<Foo>(null, 1);

			Assert.IsInstanceOfType(foo, typeof(Foo));
			Assert.AreEqual(1, foo.Id);
			Assert.IsNull(foo.Name);
		}

		[TestMethod]
		public void FirstSignature_NullValueType_ThrowsMissingMethodException()
		{
			var creator = new ObjectCreator();

			Assert.ThrowsException<MissingMethodException>(() => creator.CreateInstance<Foo>(null, "one"));
		}

		[TestMethod]
		public void SecondSignature_NullValueType_ThrowsMissingMethodException()
		{
			var creator = new ObjectCreator();

			Assert.ThrowsException<MissingMethodException>(() => creator.CreateInstance<Foo>("one", null));
		}

		private class Foo
		{
			public int Id { get; private set; }

			public string Name { get; private set; }

			public Foo(int id, string name)
			{
				Id = id;
				Name = name;
			}

			public Foo(string name, int id)
			{
				Name = name;
				Id = id;
			}
		}
	}

	[TestClass]
	public class CreateInstance_TwoReferenceTypeParameters
	{
		[TestMethod]
		public void FirstSignature_CreatesInstance()
		{
			var creator = new ObjectCreator();
			var bar = new Bar();
			var foo = creator.CreateInstance<Foo>("one", bar);

			Assert.IsInstanceOfType(foo, typeof(Foo));
			Assert.AreEqual("one", foo.Name);
			Assert.AreEqual(bar, foo.Bar);
		}

		[TestMethod]
		public void SecondSignature_CreatesInstance()
		{
			var creator = new ObjectCreator();
			var bar = new Bar();
			var foo = creator.CreateInstance<Foo>(bar, "one");

			Assert.IsInstanceOfType(foo, typeof(Foo));
			Assert.AreEqual("one", foo.Name);
			Assert.AreEqual(bar, foo.Bar);
		}

		[TestMethod]
		public void FirstSignature_NullFirstParameter_CreatesInstance()
		{
			var creator = new ObjectCreator();
			var bar = new Bar();
			var foo = creator.CreateInstance<Foo>(null, bar);

			Assert.IsInstanceOfType(foo, typeof(Foo));
			Assert.IsNull(foo.Name);
			Assert.AreEqual(bar, foo.Bar);
		}

		[TestMethod]
		public void FirstSignature_NullSecondParameter_CreatesInstance()
		{
			var creator = new ObjectCreator();
			var foo = creator.CreateInstance<Foo>("one", null);

			Assert.IsInstanceOfType(foo, typeof(Foo));
			Assert.AreEqual("one", foo.Name);
			Assert.IsNull(foo.Bar);
		}

		[TestMethod]
		public void SecondSignature_NullFirstParameter_CreatesInstance()
		{
			var creator = new ObjectCreator();
			var foo = creator.CreateInstance<Foo>(null, "one");

			Assert.IsInstanceOfType(foo, typeof(Foo));
			Assert.IsNull(foo.Bar);
			Assert.AreEqual("one", foo.Name);
		}

		[TestMethod]
		public void SecondSignature_NullSecondParameter_CreatesInstance()
		{
			var creator = new ObjectCreator();
			var bar = new Bar();
			var foo = creator.CreateInstance<Foo>(bar, null);

			Assert.IsInstanceOfType(foo, typeof(Foo));
			Assert.AreEqual(bar, foo.Bar);
			Assert.IsNull(foo.Name);
		}

		[TestMethod]
		public void FirstSignature_BothNullParameters_ThrowsAmbiguousMatchException()
		{
			var creator = new ObjectCreator();

			Assert.ThrowsException<AmbiguousMatchException>(() => creator.CreateInstance<Foo>(null, null));
		}

		private class Foo
		{
			public string Name { get; set; }

			public Bar Bar { get; set; }

			public Foo(string name, Bar bar)
			{
				Name = name;
				Bar = bar;
			}

			public Foo(Bar bar, string name)
			{
				Bar = bar;
				Name = name;
			}
		}

		private class Bar { }
	}

	[TestClass]
	public class CreateInstance_PrivateConstructor
	{
		[TestMethod]
		public void CreatesInstance()
		{
			var creator = new ObjectCreator();

			var foo = creator.CreateInstance<Foo>();

			Assert.IsInstanceOfType(foo, typeof(Foo));
		}

		private class Foo
		{
			private Foo() { }
		}
	}

	//[TestClass]
	public class CreateInstance_GenericType
	{
		[TestMethod]
		public void Test1()
		{
			var creator = new ObjectCreator();

			creator.CreateInstance<Foo<string>>();
		}

		private static object RunGenericCreateInstance(Type type)
		{
			var methodInfo = typeof(ReflectionHelper)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.FirstOrDefault(m => m.Name.Equals(@"CreateInstance", StringComparison.Ordinal) && m.IsGenericMethod)?
				.MakeGenericMethod(type);

			return methodInfo.Invoke(null, new[] { new object[0] });
		}

		private class Foo<T> { }
	}

	//[TestClass]
	public class CreateInstance_DynamicType
	{
		[TestMethod]
		public void DifferentRunTimeTypesWithSameAssemblyQualifiedNameTest()
		{
			var creator = new ObjectCreator();

			Type type1 = GenerateDynamicType();
			Type type2 = GenerateDynamicType();

			Debug.Assert(type1.AssemblyQualifiedName.Equals(type2.AssemblyQualifiedName, StringComparison.Ordinal), @"The two generated dynamic types should have same assembly qualified name.");
			Debug.Assert(type1.GetHashCode() != type2.GetHashCode(), @"The two generated dynamic types should have different hash codes.");

			var instance1 = creator.CreateInstance(type1);

			Assert.IsNotNull(instance1);
			Assert.IsInstanceOfType(instance1, type1);
			Assert.IsNotInstanceOfType(instance1, type2);

			var instance2 = creator.CreateInstance(type2);

			Assert.IsNotNull(instance2);
			Assert.IsInstanceOfType(instance2, type2);
			Assert.IsNotInstanceOfType(instance2, type1);

			instance1 = RunGenericCreateInstance(type1);

			Assert.IsNotNull(instance1);
			Assert.IsInstanceOfType(instance1, type1);
			Assert.IsNotInstanceOfType(instance1, type2);

			instance2 = RunGenericCreateInstance(type2);

			Assert.IsNotNull(instance2);
			Assert.IsInstanceOfType(instance2, type2);
			Assert.IsNotInstanceOfType(instance2, type1);
		}

		private static object RunGenericCreateInstance(Type type)
		{
			var methodInfo = typeof(ReflectionHelper)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.FirstOrDefault(m => m.Name.Equals(@"CreateInstance", StringComparison.Ordinal) && m.IsGenericMethod)?
				.MakeGenericMethod(type);

			Debug.Assert(methodInfo != null, "The generic method instance should not be null.");

			return methodInfo.Invoke(null, new[] { new object[0] });
		}

		private static Type GenerateDynamicType()
		{
			var assemblyName = new AssemblyName("DynamicAssemblyForCsvHelperTest");
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
			var typeBuilder = moduleBuilder.DefineType("DynamicTypeForCsvHelperTest", TypeAttributes.Public);

			return typeBuilder.CreateType();
		}
	}

	//[TestClass]
	public class ResolveInterfacesTests
	{
		[TestMethod]
		public void InterfaceReferenceMappingTest()
		{
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			using (var reader = new StreamReader(stream))
			using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
			{
				csv.Context.ObjectResolver = new TestContractResolver();
				csv.Configuration.Delimiter = ",";
				writer.WriteLine("AId,BId,CId,DId");
				writer.WriteLine("1,2,3,4");
				writer.Flush();
				stream.Position = 0;

				csv.Configuration.RegisterClassMap<AMap>();
				var records = csv.GetRecords<IA>().ToList();
			}
		}

		[TestMethod]
		public void InterfacePropertySubMappingTest()
		{
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			using (var reader = new StreamReader(stream))
			using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
			{
				csv.Context.ObjectResolver = new TestContractResolver();
				csv.Configuration.Delimiter = ",";
				writer.WriteLine("AId,BId,CId,DId");
				writer.WriteLine("1,2,3,4");
				writer.Flush();
				stream.Position = 0;

				csv.Configuration.RegisterClassMap<ASubPropertyMap>();
				var records = csv.GetRecords<IA>().ToList();
			}
		}

		[TestMethod]
		public void InterfaceAutoMappingTest()
		{
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			using (var reader = new StreamReader(stream))
			using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
			{
				csv.Context.ObjectResolver = new TestContractResolver();
				csv.Configuration.Delimiter = ",";
				writer.WriteLine("AId,BId,CId,DId");
				writer.WriteLine("1,2,3,4");
				writer.Flush();
				stream.Position = 0;

				var records = csv.GetRecords<IA>().ToList();
			}
		}

		private class TestContractResolver : IObjectResolver
		{
			private readonly ObjectCreator objectCreator = new ObjectCreator();

			public Func<Type, bool> CanResolve { get; set; }

			public Func<Type, object[], object> ResolveFunction { get; set; }

			public bool UseFallback { get; set; }

			public object Resolve(Type type, params object[] constructorArgs)
			{
				if (type == typeof(IA))
				{
					return new A();
				}

				if (type == typeof(IB))
				{
					return new B();
				}

				if (type == typeof(IC))
				{
					return new C();
				}

				if (type == typeof(ID))
				{
					return new D();
				}

				return objectCreator.CreateInstance(type, constructorArgs);
			}

			public T Resolve<T>(params object[] constructorArgs)
			{
				return (T)Resolve(typeof(T), constructorArgs);
			}
		}

		private interface IA
		{
			int AId { get; set; }

			IB B { get; set; }
		}

		private interface IB
		{
			int BId { get; set; }

			IC C { get; set; }
		}

		private interface IC
		{
			int CId { get; set; }

			ID D { get; set; }
		}

		private interface ID
		{
			int DId { get; set; }
		}

		private class A : IA
		{
			public int AId { get; set; }

			public IB B { get; set; }
		}

		private class B : IB
		{
			public int BId { get; set; }

			public IC C { get; set; }
		}

		private class C : IC
		{
			public int CId { get; set; }

			public ID D { get; set; }
		}

		private class D : ID
		{
			public int DId { get; set; }
		}

		private sealed class ASubPropertyMap : ClassMap<IA>
		{
			public ASubPropertyMap()
			{
				Map(m => m.AId);
				Map(m => m.B.BId);
				Map(m => m.B.C.CId);
				Map(m => m.B.C.D.DId);
			}
		}

		private sealed class AMap : ClassMap<IA>
		{
			public AMap()
			{
				Map(m => m.AId);
				References<BMap>(m => m.B);
			}
		}

		private sealed class BMap : ClassMap<IB>
		{
			public BMap()
			{
				Map(m => m.BId);
				References<CMap>(m => m.C);
			}
		}

		private sealed class CMap : ClassMap<IC>
		{
			public CMap()
			{
				Map(m => m.CId);
				References<DMap>(m => m.D);
			}
		}

		private sealed class DMap : ClassMap<ID>
		{
			public DMap()
			{
				Map(m => m.DId);
			}
		}
	}
}

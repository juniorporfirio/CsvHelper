using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CsvHelper.Tests.ObjectCreatorTests
{
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
			public int Id { get; set; }

			public Foo() { }
		}
	}

	[TestClass]
	public class CreateInstance_OneParameterConstructorAndDefaultConstructor
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
		public void OneParameterWrongType_Throws()
		{
			var creator = new ObjectCreator();

			Assert.ThrowsException<MissingMethodException>(() => creator.CreateInstance<Foo>(string.Empty));
		}

		private class Foo
		{
			public int Id { get; set; }

			public Foo() { }

			public Foo(int id)
			{
				Id = id;
			}
		}
	}

    public class CreateReferenceTypeTests
    {
        public void Test()
		{
			var creator = new ObjectCreator();
			creator.CreateInstance<Foo>(1);
			creator.CreateInstance<Foo>("one");
			creator.CreateInstance<Foo>(1, "one");
			creator.CreateInstance<Foo>("", new Bar());
			creator.CreateInstance<Foo>(new Bar(), "");
			creator.CreateInstance<Foo>(null, new Bar());
			creator.CreateInstance<Foo>("", null);

			//Assert.ThrowsException<AmbiguousMatchException>(() => creator.CreateInstance<Foo>(null, null));
		}

		public void CreateInstance_OnlyDefaultConstructor_CreatesInstance()
		{
			var creator = new ObjectCreator();
			var instance = creator.CreateInstance<OnlyDefault>();

			//Assert.IsInstanceOfType(instance, typeof(OnlyDefault));
			//Assert.AreEqual(default(int), instance.Id);
		}

		private class OnlyDefault
		{
			public int Id { get; set; }

			public OnlyDefault() { }
		}

		private class NoDefault
		{
			public int Id { get; set; }

			public NoDefault(int id)
			{
				Id = id;
			}
		}

		private class HasDefault
		{
			public int Id { get; set; }

			public HasDefault() { }

			public HasDefault(int id)
			{
				Id = id;
			}
		}

		private class NoDefaultSameArgCount
		{
			public int Id { get; set; }

			public string Name { get; set; }

			public NoDefaultSameArgCount(int id)
			{
				Id = id;
			}

			public NoDefaultSameArgCount(string name)
			{
				Name = name;
			}
		}

		private class NoDefaultDifferentArgCount
		{
			public int Id { get; set; }

			public string Name { get; set; }

			public NoDefaultDifferentArgCount(int id)
			{
				Id = id;
			}

			public NoDefaultDifferentArgCount(int id, string name)
			{
				Name = name;
			}
		}

		private class HasDefaultSameArgCount
		{
			public int Id { get; set; }

			public string Name { get; set; }

			public HasDefaultSameArgCount() { }

			public HasDefaultSameArgCount(int id)
			{
				Id = id;
			}

			public HasDefaultSameArgCount(string name)
			{
				Name = name;
			}
		}

		private class HasDefaultDifferentArgCount
		{
			public int Id { get; set; }

			public string Name { get; set; }

			public HasDefaultDifferentArgCount() { }

			public HasDefaultDifferentArgCount(int id)
			{
				Id = id;
			}

			public HasDefaultDifferentArgCount(int id, string name)
			{
				Name = name;
			}
		}

		private class NoDefaultSameArgCountDifferentSignatures
		{
			public int Id { get; set; }

			public string Name { get; set; }

			public NoDefaultSameArgCountDifferentSignatures(int id, string name)
			{
				Id = id;
				Name = name;
			}

			public NoDefaultSameArgCountDifferentSignatures(string name, int id)
			{
				Name = name;
				Id = id;
			}
		}

		private class HasDefaultSameArgCountDifferentSignatures
		{
			public int Id { get; set; }

			public string Name { get; set; }

			public HasDefaultSameArgCountDifferentSignatures() { }

			public HasDefaultSameArgCountDifferentSignatures(int id, string name)
			{
				Id = id;
				Name = name;
			}

			public HasDefaultSameArgCountDifferentSignatures(string name, int id)
			{
				Name = name;
				Id = id;
			}
		}

		private class Foo
		{
			public int Id { get; set; }
			public string Name { get; set; }

			public Foo(int id)
			{
				Id = id;
			}

			public Foo(string name)
			{
				Name = name;
			}

			public Foo(int id, string name)
			{
				Id = id;
				Name = name;
			}

			public Foo(string s, Bar b) { }

			public Foo(Bar b, string s) { }
		}

		private class Bar { }
    }
}

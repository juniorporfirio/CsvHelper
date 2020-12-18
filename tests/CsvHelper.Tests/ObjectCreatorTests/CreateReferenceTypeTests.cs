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
    public class CreateReferenceTypeTests
    {
		[TestMethod]
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

			Assert.ThrowsException<AmbiguousMatchException>(() => creator.CreateInstance<Foo>(null, null));
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

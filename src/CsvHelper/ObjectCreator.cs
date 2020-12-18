using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CsvHelper
{
	/// <summary>
	/// Efficiently creates instances of object types.
	/// </summary>
	public class ObjectCreator
    {
		private Dictionary<int, Func<object[], object>[]> funcCache = new Dictionary<int, Func<object[], object>[]>();
		private Dictionary<int, Constructors> constructorCache = new Dictionary<int, Constructors>();

		/// <summary>
		/// Creates an instance of type T using the given arguments.
		/// </summary>
		/// <typeparam name="T">The type to create an instance of.</typeparam>
		/// <param name="args">The constrcutor arguments.</param>
		public T CreateInstance<T>(params object[] args)
		{
			return (T)CreateInstance(typeof(T), args);
		}

		/// <summary>
		/// Creates an instance of the given type using the given arguments.
		/// </summary>
		/// <param name="type">The type to create an instance of.</param>
		/// <param name="args">The constructor arguments.</param>
		public object CreateInstance(Type type, params object[] args)
		{
			var func = GetFunc(type, args);

			return func(args);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Func<object[], object> GetFunc(Type type, object[] args)
		{
			var funcCacheKey = GetFuncCacheKey(type, args.Length);

			if (!funcCache.TryGetValue(funcCacheKey, out var funcs))
			{
				// If the cache doesn't exist for one type/signature combo, it doesn't for any of that type.
				CreateCaches(type);
				funcs = funcCache[funcCacheKey];
			}

			if (funcs.Length == 1)
			{
				return funcs[0];
			}

			// There is more than one constructor that matches the arg count.
			// We need to match up the signatures.

			var constructorCacheKey = GetConstructorCacheKey(type);
			var constructors = constructorCache[constructorCacheKey];
			var constructor = constructors.GetConstructor(args);

			return constructor.Func;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CreateCaches(Type type)
		{
			var constructors = new Constructors(type);

			// Generate func cache.
			foreach (var pair in constructors.ByParameterCount)
			{
				var funcs = new Func<object[], object>[pair.Value.Count];
				for (var i = 0; i < pair.Value.Count; i++)
				{
					funcs[i] = pair.Value[i].Func;
				}

				var funcCacheKey = GetFuncCacheKey(type, pair.Key);
				funcCache[funcCacheKey] = funcs;
			}

			// Generate constructor cache.
			var constructorCacheKey = GetConstructorCacheKey(type);
			constructorCache[constructorCacheKey] = constructors;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetFuncCacheKey(Type type, int argsCount)
#if !NET45
			=> HashCode.Combine(type, argsCount);
#else
		{
			unchecked
			{
				var hash = 17;
				hash = hash * 31 + type.GetHashCode();
				hash = hash * 31 + argsCount.GetHashCode();

				return hash;
			}
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetConstructorCacheKey(Type type) => type.GetHashCode();

		private static Func<object[], object> CreateInstanceFunc(Type type, Type[] parameterTypes)
		{
			var constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, parameterTypes, null);

			var parameterExpression = Expression.Parameter(typeof(object[]), "args");

			var arguments = new List<Expression>();
			for (var i = 0; i < parameterTypes.Length; i++)
			{
				var parameterType = parameterTypes[i];
				var arrayIndexExpression = Expression.ArrayIndex(parameterExpression, Expression.Constant(i));
				var convertExpression = Expression.Convert(arrayIndexExpression, parameterType);
				arguments.Add(convertExpression);
			}

			var constructorExpression = Expression.New(constructor, arguments);
			var lambda = Expression.Lambda<Func<object[], object>>(constructorExpression, new[] { parameterExpression });
			var func = lambda.Compile();

			return func;
		}

		private class Constructors
		{
			public readonly Dictionary<int, List<Constructor>> ByParameterCount = new Dictionary<int, List<Constructor>>();

			public Constructors(Type type)
			{
				var constructorInfos = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

				for (var i = 0; i < constructorInfos.Length; i++)
				{
					var constructor = new Constructor(type, constructorInfos[i]);

					var key = constructor.ParameterTypes.Length;
					if (!ByParameterCount.ContainsKey(key))
					{
						ByParameterCount[key] = new List<Constructor>();
					}

					ByParameterCount[key].Add(constructor);
				}
			}

			public Constructor GetConstructor(object[] args)
			{
				var key = args.Length;
				var constructors = ByParameterCount[key];
				if (constructors.Count == 1)
				{
					return constructors[0];
				}

				// There is more than one constructor with the same amount of parameters.
				// Match against the types.

				var argsTypes = new Type[args.Length];
				for (var i = 0; i < args.Length; i++)
				{
					argsTypes[i] = args[i]?.GetType() ?? typeof(object);
				}

				var fuzzyMatches = new List<Constructor>();
				for (var i = 0; i < constructors.Count; i++)
				{
					var constructor = constructors[i];
					var matchType = MatchType.Exact;

					for (var j = 0; j < args.Length; j++)
					{
						var parameterType = constructor.ParameterTypes[j];
						var argType = argsTypes[j];
						if (args[j] != null && parameterType == argType)
						{
							continue;
						}

						if (args[j] == null && !parameterType.IsValueType)
						{
							matchType = MatchType.Fuzzy;
							continue;
						}

						matchType = MatchType.None;
						break;
					}

					if (matchType == MatchType.Exact)
					{
						// It's only possible to have a single exact match.
						return constructor;
					}

					if (matchType == MatchType.Fuzzy)
					{
						fuzzyMatches.Add(constructor);
					}
				}

				if (fuzzyMatches.Count == 1)
				{
					return fuzzyMatches[0];
				}

				if (fuzzyMatches.Count > 1)
				{
					throw new AmbiguousMatchException();
				}

				throw new InvalidOperationException("There is no constructor signature that matches the given args.");
			}

			private enum MatchType
			{
				None = 0,
				Exact = 1,
				Fuzzy = 2,
			}
		}

		private class Constructor
		{
			public Func<object[], object> Func { get; private set; }

			public Type[] ParameterTypes { get; private set; }

			public Constructor(Type type, ConstructorInfo constructor)
			{
				var parameters = constructor.GetParameters();
				ParameterTypes = new Type[parameters.Length];

				for (var i = 0; i < parameters.Length; i++)
				{
					ParameterTypes[i] = parameters[i].ParameterType;
				}

				Func = CreateInstanceFunc(type, ParameterTypes);
			}
		}
	}
}

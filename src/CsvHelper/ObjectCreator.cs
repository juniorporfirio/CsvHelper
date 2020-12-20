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
		private readonly Dictionary<int, Constructor[]> constructorCache = new Dictionary<int, Constructor[]>();
		private readonly HashSet<int> cachedTypes = new HashSet<int>();
		private static readonly int objectHashCode = typeof(object).GetHashCode();

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
			var key = GetConstructorCacheKey(type, args.Length);
			if (!constructorCache.TryGetValue(key, out var constructors))
			{
				if (!cachedTypes.Contains(type.GetHashCode()))
				{
					CreateCache(type);
				}

				if (!constructorCache.ContainsKey(key))
				{
					throw GetConstructorNotFoundException(type, args);
				}

				constructors = constructorCache[key];
			}

			var constructor = GetConstructor(constructors, type, args);

			return constructor.Func;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetConstructorCacheKey(Type type, int argsCount)
		{ 
#if !NET45
			return HashCode.Combine(type, argsCount);
#else
			unchecked
			{
				var hash = 17;
				hash = hash * 31 + type.GetHashCode();
				hash = hash * 31 + argsCount.GetHashCode();

				return hash;
			}
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CreateCache(Type type)
		{
			var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			var cache = new Dictionary<int, List<Constructor>>();
			for (var i = 0; i < constructors.Length; i++)
			{
				var constructor = new Constructor(type, constructors[i]);

				var key = GetConstructorCacheKey(type, constructor.ParameterTypeHashCodes.Length);
				if (!cache.ContainsKey(key))
				{
					cache[key] = new List<Constructor>();
				}

				cache[key].Add(constructor);
			}

			foreach (var pair in cache)
			{
				constructorCache[pair.Key] = pair.Value.ToArray();
			}

			cachedTypes.Add(type.GetHashCode());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Constructor GetConstructor(Constructor[] constructors, Type type, object[] args)
		{
			// Match the signature.

			var argsTypes = new int[args.Length];
			for (var i = 0; i < args.Length; i++)
			{
				argsTypes[i] = args[i]?.GetType().GetHashCode() ?? objectHashCode;
			}

			var fuzzyMatches = new List<Constructor>();
			for (var i = 0; i < constructors.Length; i++)
			{
				var constructor = constructors[i];
				var matchType = MatchType.Exact;

				for (var j = 0; j < args.Length; j++)
				{
					var parameterType = constructor.ParameterTypeHashCodes[j];
					var argType = argsTypes[j];
					if (args[j] != null && parameterType == argType)
					{
						continue;
					}

					if (args[j] == null && parameterType == objectHashCode)
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

			throw GetConstructorNotFoundException(type, args);
		}

		private MissingMethodException GetConstructorNotFoundException(Type type, object[] args)
		{
			var signature = $"{type.FullName}({string.Join(", ", args.Select(a => (a?.GetType() ?? typeof(object)).FullName))})";
			return new MissingMethodException($"Constructor '{signature}' was not found.");
		}

		private enum MatchType
		{
			None = 0,
			Exact = 1,
			Fuzzy = 2,
		}

		private class Constructor
		{
			public Func<object[], object> Func { get; private set; }

			public int[] ParameterTypeHashCodes { get; private set; }

			public Constructor(Type type, ConstructorInfo constructor)
			{
				var parameters = constructor.GetParameters();
				var parameterTypes = new Type[parameters.Length];
				ParameterTypeHashCodes = new int[parameters.Length];
				for (var i = 0; i < parameters.Length; i++)
				{
					parameterTypes[i] = parameters[i].ParameterType;
					ParameterTypeHashCodes[i] = parameters[i].ParameterType.GetHashCode();
				}

				Func = CreateInstanceFunc(type, parameterTypes);
			}
		}
	}
}

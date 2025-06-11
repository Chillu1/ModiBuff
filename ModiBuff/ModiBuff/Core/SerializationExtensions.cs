using System;
using System.Collections.Generic;
using System.Linq;

namespace ModiBuff.Core
{
	public static class SerializationExtensions
	{
#if MODIBUFF_SYSTEM_TEXT_JSON
		private static readonly Dictionary<Type, Func<System.Text.Json.JsonElement, object>> customValueTypes =
			new Dictionary<Type, Func<System.Text.Json.JsonElement, object>>();

		public static void AddCustomValueType<T>(Func<System.Text.Json.JsonElement, object> converter)
		{
			AddCustomValueType(typeof(T), converter);
		}

		public static void AddCustomValueType(Type type, Func<System.Text.Json.JsonElement, object> converter)
		{
			customValueTypes.Add(type, converter);
		}

		public static bool FromAnonymousJsonObjectToSaveData(this object fromLoad, ISavable toLoad)
		{
			if (fromLoad is not System.Text.Json.JsonElement jsonElement)
				return false;

			var genericType = toLoad.GetType().GetInterfaces().FirstOrDefault(x =>
				x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISavable<>));
			if (genericType == null)
			{
				Logger.LogError($"[ModiBuff] Object {toLoad.GetType()} doesn't implement ISavable<TSaveData>");
				return false;
			}

			var constructor = genericType.GetGenericArguments()[0].GetConstructors()[0];
			var constructorParameterTypes = constructor.GetParameters().Select(x => x.ParameterType).ToArray();

			int i = 0;
			object?[] parameters = jsonElement.EnumerateObject()
				.Select(j => j.Value.ToValue(constructorParameterTypes[i++]).Value)
				.ToArray();

			toLoad.LoadState(constructor.Invoke(parameters));
			return true;
		}

		public static bool TryGetDataFromJsonObject<T>(this object fromLoad, out T data)
		{
			if (fromLoad is not System.Text.Json.JsonElement jsonElement)
			{
				data = default;
				return false;
			}

			data = (T)jsonElement.ToValue(typeof(T)).Value!;
			return true;
		}

		public static T GetDataFromJsonObject<T>(this object fromLoad)
		{
			return TryGetDataFromJsonObject(fromLoad, out T data) ? data : default;
		}

		private static readonly List<object> valuesHolder = new List<object>();

		public static object[] GetValues(this System.Text.Json.JsonElement element, params Type[] types)
		{
			const int maxValues = 16;
			int i = 0;
			foreach (var value in element.EnumerateObject())
			{
				for (int j = 0; j < maxValues; j++)
				{
					if (i >= types.Length)
						break;

					if (value.NameEquals($"Item{j + 1}"))
					{
						valuesHolder.Add(value.Value.ToValue(types[i]));
						break;
					}
				}

				i++;
			}

			object[] values = valuesHolder.ToArray();
			valuesHolder.Clear();
			return values;
		}

		public static (bool Success, object? Value) ToValue(this System.Text.Json.JsonElement element, Type type)
		{
			if (type == typeof(int))
				return (true, element.GetInt32());
			if (type == typeof(float))
				return (true, element.GetSingle());
			if (type == typeof(bool))
				return (true, element.GetBoolean());
			if (type == typeof(double))
				return (true, element.GetDouble());
			if (type == typeof(long))
				return (true, element.GetInt64());
			if (type == typeof(byte))
				return (true, element.GetByte());
			if (type == typeof(string))
				return (true, element.GetString());
			if (type == typeof(uint))
				return (true, element.GetUInt32());
			if (type == typeof(ushort))
				return (true, element.GetUInt16());
			if (type == typeof(ulong))
				return (true, element.GetUInt64());
			if (type == typeof(sbyte))
				return (true, element.GetSByte());
			if (type == typeof(short))
				return (true, element.GetInt16());
			if (type == typeof(decimal))
				return (true, element.GetDecimal());
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				var underlyingType = Nullable.GetUnderlyingType(type);
				if (underlyingType == null)
					return (true, null);

				if (element.ValueKind == System.Text.Json.JsonValueKind.Null)
					return (true, null);

				return element.ToValue(underlyingType);
			}

			if (type == typeof(IReadOnlyDictionary<int, int>))
			{
				var dictionary = new Dictionary<int, int>();
				foreach (var kvp in element.EnumerateObject())
					dictionary.Add(int.Parse(kvp.Name), kvp.Value.GetInt32());
				return (true, dictionary);
			}

			foreach (var kvp in customValueTypes)
			{
				if (type == kvp.Key)
					return (true, kvp.Value(element));
			}

			if (type.IsEnum)
				return element.ToValue(Enum.GetUnderlyingType(type));

			if (type == typeof(object))
				return (true, element.GetRawText());

			Logger.LogWarning($"[ModiBuff] Unknown ValueType {type}");
			return (false, null);
		}
#endif
	}
}
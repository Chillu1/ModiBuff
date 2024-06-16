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
			object[] parameters = jsonElement.EnumerateObject()
				.Select(j => j.Value.ToValue(constructorParameterTypes[i++]))
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

			data = (T)jsonElement.ToValue(typeof(T));
			return true;
		}

		public static T GetDataFromJsonObject<T>(this object fromLoad)
		{
			return TryGetDataFromJsonObject(fromLoad, out T data) ? data : default;
		}

		public static object GetDataFromJsonObject(this object fromLoad, Type type)
		{
			return fromLoad is not System.Text.Json.JsonElement jsonElement ? null : jsonElement.ToValue(type);
		}

		private static object ToValue(this System.Text.Json.JsonElement element, Type type)
		{
			if (type == typeof(int))
				return element.GetInt32();
			if (type == typeof(float))
				return element.GetSingle();
			if (type == typeof(bool))
				return element.GetBoolean();
			if (type == typeof(double))
				return element.GetDouble();
			if (type == typeof(long))
				return element.GetInt64();
			if (type == typeof(byte))
				return element.GetByte();
			if (type == typeof(string))
				return element.GetString();
			if (type == typeof(uint))
				return element.GetUInt32();
			if (type == typeof(ushort))
				return element.GetUInt16();
			if (type == typeof(ulong))
				return element.GetUInt64();
			if (type == typeof(sbyte))
				return element.GetSByte();
			if (type == typeof(short))
				return element.GetInt16();
			if (type == typeof(decimal))
				return element.GetDecimal();
			if (type == typeof(IReadOnlyDictionary<int, int>))
			{
				var dictionary = new Dictionary<int, int>();
				foreach (var kvp in element.EnumerateObject())
					dictionary.Add(int.Parse(kvp.Name), kvp.Value.GetInt32());
				return dictionary;
			}

			foreach (var kvp in customValueTypes)
			{
				if (type == kvp.Key)
					return kvp.Value(element);
			}

			if (type == typeof(object))
				return element.GetRawText();

			Logger.LogWarning($"[ModiBuff] Unknown ValueType {type}");
			return null;
		}
#endif
	}
}
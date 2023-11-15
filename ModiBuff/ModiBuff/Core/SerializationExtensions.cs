using System;
using System.Linq;

namespace ModiBuff.Core
{
	public static class SerializationExtensions
	{
#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET462_OR_GREATER || NETCOREAPP2_1_OR_GREATER)
		public static bool FromAnonymousJsonObjectToSaveData(this object fromLoad, ISavable toLoad)
		{
			if (!(fromLoad is System.Text.Json.JsonElement jsonElement))
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

			Logger.LogError($"[ModiBuff] Unknown ValueType {type}");
			return null;
		}
#endif
	}
}
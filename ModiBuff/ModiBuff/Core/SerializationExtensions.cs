using System;
using System.Linq;

namespace ModiBuff.Core
{
	public static class SerializationExtensions
	{
#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER)
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

			object[] parameters = jsonElement.EnumerateObject().Select(j => j.Value.ReturnUnderlyingType())
				.ToArray();
			object saveData = Activator.CreateInstance(genericType.GetGenericArguments()[0], parameters);
			toLoad.LoadState(saveData);
			return true;
		}

		public static object ReturnUnderlyingType(this System.Text.Json.JsonElement element)
		{
			switch (element.ValueKind)
			{
				//TODO int, double, etc
				case System.Text.Json.JsonValueKind.String:
					return element.GetString();
				case System.Text.Json.JsonValueKind.True:
					return true;
				case System.Text.Json.JsonValueKind.False:
					return false;
				case System.Text.Json.JsonValueKind.Number:
				{
					//TODO Not exhaustive/ideal
					//
					if (element.TryGetDouble(out double doubleValue))
					{
						if (element.TryGetSingle(out float floatValue))
							return floatValue;
						return doubleValue;
					}

					if (element.TryGetInt64(out long longValue))
						return longValue;
					if (element.TryGetInt32(out int intValue))
						return intValue;
					break;
				}
			}

			Logger.LogError($"[ModiBuff] Unknown JsonValueKind {element.ValueKind}");
			return null;
		}
#endif
	}
}
using System;
using System.IO;
using System.Text.Json;

namespace ModiBuff.Extensions.Serialization.Json
{
	public sealed class SaveController
	{
		private readonly string _path;
		private readonly JsonSerializerOptions _options;

		public SaveController(string fileName)
		{
			_path = Path.Combine(Environment.CurrentDirectory, fileName);

			_options = new JsonSerializerOptions
			{
				WriteIndented = true, IncludeFields = true /*, Converters =
				{
					new UnitSaveDataJsonConverter()
				}*/
			};
		}

		public bool Save<T>(T obj)
		{
			File.WriteAllText(_path, JsonSerializer.Serialize(obj, _options));
			//using (var stream = File.Create(_path))
			//{
			//	JsonSerializer.Serialize(stream, obj, _options);
			//	return true;
			//}
			return true;
		}

		public T Load<T>()
		{
			return JsonSerializer.Deserialize<T>(File.ReadAllText(_path), _options);
			//using (var stream = File.OpenRead(_path))
			//	return JsonSerializer.Deserialize<T>(stream, _options);
		}
	}

	/*public class UnitSaveDataJsonConverter : JsonConverter<Unit.SaveData>
	{
		public override Unit.SaveData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			Logger.Log("Custom Read");
			var jsonObject = JsonDocument.ParseValue(ref reader).RootElement;
			var health = jsonObject.GetProperty("Health").GetSingle();
			var mana = jsonObject.GetProperty("Mana").GetSingle();
			var damage = jsonObject.GetProperty("Damage").GetSingle();
			return new Unit.SaveData(UnitTag.None, health, 0, damage, 0, mana, 0, 0,
				UnitType.Good, false, default, default, default, default);
		}

		public override void Write(Utf8JsonWriter writer, Unit.SaveData value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteNumber("Health", value.Health);
			writer.WriteNumber("Mana", value.Mana);
			writer.WriteNumber("Damage", value.Damage);
			writer.WriteEndObject();
		}
	}*/
}
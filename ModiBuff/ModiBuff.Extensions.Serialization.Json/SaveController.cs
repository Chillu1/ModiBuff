using System;
using System.IO;
using System.Text.Json;
using ModiBuff.Core.Units;

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

		public string Save(Unit.SaveData obj) => JsonSerializer.Serialize(obj, _options);

		public void SaveToFile(Unit.SaveData obj) => SaveToFile(Save(obj));
		public void SaveToFile(string json) => File.WriteAllText(_path, json);

		public Unit.SaveData Load(string json) => JsonSerializer.Deserialize<Unit.SaveData>(json, _options);

		public Unit.SaveData LoadFromFile() => JsonSerializer.Deserialize<Unit.SaveData>(LoadFromFileJson(), _options);
		public string LoadFromFileJson() => File.ReadAllText(_path);
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
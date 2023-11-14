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

			_options = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
		}

		public string Save(Unit.SaveData obj) => JsonSerializer.Serialize(obj, _options);

		public void SaveToFile(Unit.SaveData obj) => SaveToFile(Save(obj));
		public void SaveToFile(string json) => File.WriteAllText(_path, json);

		public Unit.SaveData Load(string json) => JsonSerializer.Deserialize<Unit.SaveData>(json, _options);

		public Unit.SaveData LoadFromFile() => JsonSerializer.Deserialize<Unit.SaveData>(LoadFromFileJson(), _options);
		public string LoadFromFileJson() => File.ReadAllText(_path);
	}
}
using System;
using System.IO;
using System.Text.Json;
using ModiBuff.Core.Units;

namespace ModiBuff.Extensions.Serialization.Json
{
	public sealed class SaveController
	{
		public readonly string Path;
		private readonly string _fileName;
		private readonly JsonSerializerOptions _options;

		public SaveController(string fileName)
		{
			Path = Environment.CurrentDirectory;
			_fileName = fileName;

			_options = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
		}

		public string Save<T>(T obj) => JsonSerializer.Serialize(obj, _options);
		public string Save(Unit.SaveData obj) => JsonSerializer.Serialize(obj, _options);

		public void SaveToFile(Unit.SaveData obj) => SaveToFile(Save(obj));

		public void SaveToFile(string json) => File.WriteAllText(System.IO.Path.Combine(Path, _fileName), json);

		public void SaveToPath(string json, string fileName) =>
			File.WriteAllText(System.IO.Path.Combine(Path, fileName), json);

		public T Load<T>(string json) => JsonSerializer.Deserialize<T>(json, _options);
		public Unit.SaveData Load(string json) => JsonSerializer.Deserialize<Unit.SaveData>(json, _options);

		public Unit.SaveData LoadFromFile() => JsonSerializer.Deserialize<Unit.SaveData>(LoadFromFileJson(), _options);

		public T LoadFromPath<T>(string fileName) =>
			JsonSerializer.Deserialize<T>(File.ReadAllText(System.IO.Path.Combine(Path, fileName)), _options);

		public string LoadFromFileJson() => File.ReadAllText(System.IO.Path.Combine(Path, _fileName));
	}
}
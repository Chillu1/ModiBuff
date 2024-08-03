using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModiBuff.Core.Units;

namespace ModiBuff.Extensions.Serialization.Json
{
	public sealed class SaveController
	{
		public readonly string Path;
		private readonly JsonSerializerOptions _options;

		public SaveController(string fileName)
		{
			Path = Environment.CurrentDirectory;

			_options = new JsonSerializerOptions
			{
				WriteIndented = true, IncludeFields = true,
				//DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
			};
		}

		public string Save<T>(T obj) => JsonSerializer.Serialize(obj, obj.GetType(), _options);
		public string Save(GameState.SaveData obj) => JsonSerializer.Serialize(obj, obj.GetType(), _options);

		public void SaveToPath(string json, string fileName) =>
			File.WriteAllText(System.IO.Path.Combine(Path, fileName), json);

		public T Load<T>(string json) => JsonSerializer.Deserialize<T>(json, _options);
		public GameState.SaveData Load(string json) => JsonSerializer.Deserialize<GameState.SaveData>(json, _options);

		public T LoadFromPath<T>(string fileName) =>
			JsonSerializer.Deserialize<T>(File.ReadAllText(System.IO.Path.Combine(Path, fileName)), _options);
	}
}
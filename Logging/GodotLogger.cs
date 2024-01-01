#if GODOT
using Godot;

namespace ModiBuff.Core
{
	public class GodotLogger : ILogger
	{
		public void Log(string message) => GD.Print(message);

		public void LogWarning(string message) => GD.Print($"[WARNING] {message}");

		public void LogError(string message) => GD.PushError($"[ERROR] {message}");
	}
}
#endif
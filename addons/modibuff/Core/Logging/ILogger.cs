namespace ModiBuff.Core
{
	public interface ILogger
	{
		void Log(string message);
		void LogWarning(string message);
		void LogError(string message);
	}
}
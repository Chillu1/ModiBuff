namespace ModiBuff.Core
{
	public static class Logger
	{
		private static ILogger _logger;

		public static void SetLogger(ILogger logger) => _logger = logger;
		public static void SetLogger<T>(T logger) where T : ILogger => _logger = logger;
		public static void SetLogger<T>() where T : ILogger, new() => _logger = new T();

		public static void Log(string message) => _logger.Log(message);
		public static void LogWarning(string message) => _logger.LogWarning(message);
		public static void LogError(string message) => _logger.LogError(message);
	}
}
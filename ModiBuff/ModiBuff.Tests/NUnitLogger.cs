using ModiBuff.Core;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class NUnitLogger : ILogger
	{
		public void Log(string message) => TestContext.WriteLine(message);

		public void LogWarning(string message) => TestContext.WriteLine(message);

		public void LogError(string message) => TestContext.WriteLine(message);
	}
}
using System;
using ModiBuff.Core;

namespace ModiBuff.Examples.SimpleSolo
{
	public sealed class ConsoleLogger : ILogger
	{
		public void Log(string message)
		{
#if NETSTANDARD1_3_OR_GREATER //TEMP Solution
			Console.WriteLine(message);
#endif
		}

		public void LogWarning(string message)
		{
#if NETSTANDARD1_3_OR_GREATER
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(message);
			Console.ResetColor();
#endif
		}

		public void LogError(string message)
		{
#if NETSTANDARD1_3_OR_GREATER
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(message);
			Console.ResetColor();
#endif
		}
	}
}
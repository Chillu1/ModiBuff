using System;
using ModiBuff.Core;

namespace ModiBuff.Examples.BasicConsole
{
	public static class Console
	{
		public static void GameMessage(string message)
		{
			System.Console.ForegroundColor = ConsoleColor.Blue;
			Logger.Log($"[Game] {message}");
			System.Console.ResetColor();
		}
	}
}
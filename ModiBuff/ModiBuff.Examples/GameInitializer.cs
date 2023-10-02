using System;

namespace ModiBuff.Examples
{
	public class GameInitializer
	{
		public static void Main()
		{
			//Console.WriteLine("Which example do you want to run?\n" +
			//                  "1. BasicConsole\n" +
			//                  "2. SimpleSolo\n");

			IGameController gameController = null;
			gameController = new BasicConsole.GameController();

			/*while (gameController == null)
			{
				string input = Console.ReadLine();
				switch (input)
				{
					case "BasicConsole":
					case "1":
						gameController = new BasicConsole.GameController();
						break;

					default:
						Console.WriteLine("Unknown example");
						break;
				}
			}*/

			bool running = true;
			while (running)
				running = gameController.Update();
		}
	}
}
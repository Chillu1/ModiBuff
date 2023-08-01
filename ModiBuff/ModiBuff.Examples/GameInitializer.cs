using ModiBuff.Examples.SimpleSolo;

namespace ModiBuff.Examples
{
	public class GameInitializer
	{
		public static void Main()
		{
			var gameController = new GameController();
			while (true)
			{
				gameController.Update();
			}
		}
	}
}
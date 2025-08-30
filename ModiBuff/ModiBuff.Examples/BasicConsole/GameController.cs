using System.Linq;
using ModiBuff.Core;

namespace ModiBuff.Examples.BasicConsole
{
	/// <summary>
	///		Simple solo example, of player unit fighting a single enemy unit
	/// </summary>
	public sealed class GameController : IGameController
	{
		private readonly ModifierIdManager _idManager;
		private readonly EffectTypeIdManager _effectTypeIdManager;
		private readonly ModifierRecipes _recipes;
		private readonly ModifierPool _pool;
		private readonly ModifierControllerPool _modifierControllerPool;

		private readonly Unit _player;

		private readonly EnemySpawner _enemySpawner;

		public GameController()
		{
			//We set the logger first, so ModiBuff can tell us if anything goes wrong 
			Logger.SetLogger<ConsoleLogger>();
			//(Optional) We can set any default in the static config file
			Config.PoolSize = 100;

			//These 3 classes need to always be created in the start of the game
			//The ModifierIdManage and ModifierRecipe will help us get info about the modifiers
			_idManager = new ModifierIdManager();
			_effectTypeIdManager = new EffectTypeIdManager();
			_effectTypeIdManager.RegisterAllEffectTypesInAssemblies();
			_recipes = new ModifierRecipes(_idManager, _effectTypeIdManager);
			//We need to register our recipes inside the pool, so we can pre-allocate the modifiers
			//Most likely you won't use the pool directly, but we still need to create it 
			_pool = new ModifierPool(_recipes);
			_modifierControllerPool = new ModifierControllerPool();

			//Now we've come to the game logic initialization, which will be custom for your game
			//Here we have just one player, and one enemy spawning each time the enemy dies
			_player = new Unit("Player", 100, 5);

			_enemySpawner = new EnemySpawner(_player);
			_enemySpawner.Spawn();

			//We can add modifiers to any unit at this stage (after the pool is created)
			//Here we're adding a modifier called "DoT" to the player
			//But we're adding it as an applier, and not as a normal modifier
			//This means that instead of it being applier to the player
			//it will be applied to a unit that the player attacks
			_player.AddApplierModifierNew(_idManager.GetId("DoT")!.Value, ApplierType.Attack);
			_player.AddApplierModifierNew(_idManager.GetId("InitHeal")!.Value, ApplierType.Cast);
			//_player.ModifierController.TryAddApplier(_idManager.GetId("DisarmChance"), true, ApplierType.Cast);
		}

		public bool Update()
		{
			bool isRunning = PlayerAction();
			if (isRunning == false)
				return false;

			//We're making the game turn-based, by having a constant 1 delta
			const float delta = 1;

			_player.Update(delta);
			_enemySpawner.Update(delta);

			return true;
		}

		private bool PlayerAction()
		{
			bool validAction = false;
			while (validAction == false)
			{
				Console.GameMessage("Actions: Attack - 1, Cast - 2, Info - 3, Quit - q");
				string action = System.Console.ReadLine();
				switch (action)
				{
					case "1":
						validAction = true;
						_player.AutoAttack();
						break;
					case "2":
						validAction = PlayerCastAction();
						break;
					case "3":
						InfoAction();
						break;
					case "q":
						return false;
					default:
						Console.GameMessage("Invalid action");
						break;
				}
			}

			return true;
		}

		private bool PlayerCastAction()
		{
			//Display all possible modifiers to cast, then when one was chosen, choose the target
			int[] modifierIds = _player.GetApplierCastModifierIds().ToArray();

			while (true)
			{
				Console.GameMessage("Choose modifier to cast, or c to cancel");
				for (int i = 0; i < modifierIds.Length; i++)
				{
					var modifierInfo = _recipes.GetModifierInfo(modifierIds[i]);
					Console.GameMessage($"{i + 1} - {modifierInfo.DisplayName} - {modifierInfo.Description}");
				}

				string castAction = System.Console.ReadLine();
				if (int.TryParse(castAction, out int castActionInt))
				{
					if (castActionInt > 0 && castActionInt <= modifierIds.Length)
					{
						//TODO: choosing target
						_player.TryApply(modifierIds[castActionInt - 1], _player);
						break;
					}
				}

				if (castAction == "c")
					return false;

				Console.GameMessage("Invalid action");
			}

			return true;
		}

		private void InfoAction()
		{
			while (true)
			{
				Console.GameMessage("Which unit to inspect? Player - 1, Enemy - 2, Back - b");
				string infoAction = System.Console.ReadLine();
				switch (infoAction)
				{
					case "1":
						_player.PrintStateAndModifiers(_recipes);
						return;
					case "2":

						return;
					case "b":
						return;
					default:
						Console.GameMessage("Invalid action");
						break;
				}
			}
		}
	}
}
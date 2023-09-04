using System.Collections.Generic;
using ModiBuff.Core;

namespace ModiBuff.Examples.BasicConsole
{
	/// <summary>
	///		Simple solo example, of player unit fighting a single enemy unit
	/// </summary>
	public sealed class GameController : IGameController
	{
		private readonly ModifierRecipes _recipes;
		private readonly ModifierPool _pool;
		//private readonly int _defenseModifierId;

		private readonly Unit _player;

		private readonly EnemySpawner _enemySpawner;

		public GameController()
		{
			Logger.SetLogger<ConsoleLogger>();
			Config.PoolSize = 100;

			var idManager = new ModifierIdManager();
			_recipes = new ModifierRecipes(idManager);
			_pool = new ModifierPool(_recipes);
			//_defenseModifierId = idManager.GetId("Defense");

			_player = new Unit("Player", 100, 5);

			_enemySpawner = new EnemySpawner(_player);
			_enemySpawner.Spawn();

			_player.ModifierController.TryAddApplier(idManager.GetId("DoT"), false, ApplierType.Attack);
		}

		public void Update()
		{
			//Actions: Attack, Cast, Defend,
			Console.GameMessage("Actions: 1 - Attack, 2 - Cast, 3 - Defend");
			string action = System.Console.ReadLine();
			switch (action)
			{
				case "1":
					_player.Attack();
					break;
				case "2":
					//Console.GameMessage("Actions: 1 - Heal, 2 - Damage");
					//string castAction = System.Console.ReadLine();
					//var applierCastModifierIds = _player.ModifierController.GetApplierCastModifierIds();
					//_recipes.GetRecipe()
					break;
				case "3":
					//_player.AddModifier(_defenseModifierId, _player);
					break;
			}

			const float delta = 0.0167f;

			_player.Update(delta);

			_enemySpawner.Update(delta);
		}
	}
}
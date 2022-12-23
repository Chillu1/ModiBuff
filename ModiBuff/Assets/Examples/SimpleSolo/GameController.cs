using ModiBuff.Core;

namespace ModiBuff.Examples.SimpleSolo
{
	/// <summary>
	///		Simple solo example, of player unit fighting a single enemy unit
	/// </summary>
	public sealed class GameController
	{
		private readonly Unit _unit;
		private readonly Unit _enemy;

		public GameController()
		{
			var recipes = new ModifierRecipes();

			_unit = new Unit();
			_enemy = new Unit();

			_unit.AddApplierModifier(recipes.GetRecipe("DoT"), ApplierType.Attack);
			_enemy.AddApplierModifier(recipes.GetRecipe("InitDamage"), ApplierType.Attack);

			_unit.SetAttackTarget(_enemy);
			_unit.SetCastTarget(_enemy);
			_enemy.SetAttackTarget(_unit);
			_enemy.SetCastTarget(_unit);
		}

		public void Update()
		{
			float delta = 0.0167f; //Time.deltaTime;
			_unit.Update(delta);
			_enemy.Update(delta);
		}
	}
}
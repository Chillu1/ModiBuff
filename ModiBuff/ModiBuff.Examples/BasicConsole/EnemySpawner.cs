namespace ModiBuff.Examples.BasicConsole
{
	public sealed class EnemySpawner
	{
		private readonly Unit _playerUnit;

		private Unit _enemy;

		public EnemySpawner(Unit playerUnit)
		{
			_playerUnit = playerUnit;
		}

		public void Spawn()
		{
			_enemy = new Unit("Enemy", 20, 1);
			Console.GameMessage($"Enemy spawned with {_enemy.Health} health and {_enemy.Damage} damage");
			_enemy.SetAttackTarget(_playerUnit);

			_playerUnit.SetAttackTarget(_enemy);
		}

		public void Update(float delta)
		{
			_enemy.Update(delta);
		}
	}
}
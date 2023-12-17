namespace ModiBuff.Core.Units
{
	//TODO Should depend on/be created by the Unit?
	public sealed class Projectile : IUnit, IPosition<Vector2>, IMovable<Vector2>,
		IInitialPosition<Vector2>, ITravelDistance<Vector2>, IUpdatable
	{
		public Vector2 Position { get; private set; }
		public Vector2 InitialPosition { get; }
		public Vector2 DistanceTraveled { get; private set; }

		private readonly IUnit _source;
		private readonly int[] _modifierIds;

		public Projectile(Vector2 initialPosition, IUnit source, int modifierId)
		{
			InitialPosition = initialPosition;
			_source = source;
			Position = initialPosition;
			_modifierIds = new[] { modifierId };
		}

		public void Update(float deltaTime)
		{
			//Position += velocity * deltaTime;
		}

		public void Move(float x, float y) => Move(new Vector2(x, y));

		public void Move(Vector2 value)
		{
			Position += value;
			DistanceTraveled += Vector2.Abs(value);
		}

		public void Hit(IUnit unit)
		{
			var modifierOwner = (IModifierOwner)unit;
			foreach (int modifierId in _modifierIds)
			{
				if (!modifierId.IsLegalTarget((IUnitEntity)unit, (IUnitEntity)_source))
					continue;

				modifierOwner.ModifierController.Add(modifierId, unit, this);
			}

			//TODO IsDead, clean up/dequeue for next frame
		}
	}
}
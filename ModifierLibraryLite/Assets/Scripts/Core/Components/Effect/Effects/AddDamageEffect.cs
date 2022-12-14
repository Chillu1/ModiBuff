namespace ModifierLibraryLite.Core
{
	public sealed class AddDamageEffect : IRevertEffect
	{
		private readonly float _damage;
		public bool IsRevertible { get; }

		public AddDamageEffect(float damage, bool revertible = false)
		{
			_damage = damage;
			IsRevertible = revertible;
		}

		public void Effect(IUnit target, IUnit acter)
		{
			target.AddDamage(_damage);
		}

		public void RevertEffect(IUnit target, IUnit owner)
		{
			target.AddDamage(-_damage);
		}
	}
}
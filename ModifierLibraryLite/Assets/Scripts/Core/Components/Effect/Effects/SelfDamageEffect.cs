namespace ModifierLibraryLite.Core
{
	/// <summary>
	///		Reverse of <see cref="DamageEffect"/>
	/// </summary>
	public sealed class SelfDamageEffect : IEffect
	{
		private readonly float _damage;

		public SelfDamageEffect(float damage) => _damage = damage;

		public void Effect(IUnit target, IUnit acter)
		{
			acter.TakeDamage(_damage, target);
		}
	}
}
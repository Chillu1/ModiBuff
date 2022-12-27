namespace ModiBuff.Core
{
	/// <summary>
	///		Reverse of <see cref="DamageEffect"/>
	/// </summary>
	public sealed class ActerDamageEffect : IEffect
	{
		private readonly float _damage;

		public ActerDamageEffect(float damage) => _damage = damage;

		public void Effect(IUnit target, IUnit acter)
		{
			acter.TakeDamage(_damage, target);
		}
	}
}
namespace ModiBuff.Core.Units
{
	public interface IDamagable<THealth> : IUnit
	{
		THealth Health { get; }
		THealth MaxHealth { get; }
	}

	public interface IDamagable<THealth, TDamage, TReturnDamageInfo> : IDamagable<THealth>
	{
		TReturnDamageInfo TakeDamage(TDamage damage, IUnit source, bool triggersEvents = true);
	}
}
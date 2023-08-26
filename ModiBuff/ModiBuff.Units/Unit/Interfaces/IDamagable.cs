namespace ModiBuff.Core.Units
{
	public interface IDamagable<THealth, TMaxHealth> : IUnit
	{
		THealth Health { get; }
		TMaxHealth MaxHealth { get; }
	}

	public interface IDamagable<THealth, TMaxHealth, TDamage, TReturnDamageInfo> : IDamagable<THealth, TMaxHealth>
	{
		TReturnDamageInfo TakeDamage(TDamage damage, IUnit source, bool triggersEvents = true);
	}
}
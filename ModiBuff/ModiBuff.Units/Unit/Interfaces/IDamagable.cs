namespace ModiBuff.Core.Units
{
	public interface IDamagable<THealth, TMaxHealth>
	{
		THealth Health { get; }
		TMaxHealth MaxHealth { get; }
	}

	public interface IDamagable<THealth, TMaxHealth, TDamage, TReturnDamageInfo> : IDamagable<THealth, TMaxHealth>
	{
		TReturnDamageInfo TakeDamage(TDamage damage, IUnit source, bool triggersEvents = true);
	}

	public static class DamagableExtensions
	{
		public static float PercentageHealth(this IDamagable<float, float> damagable)
		{
			return damagable.Health / damagable.MaxHealth;
		}
	}
}
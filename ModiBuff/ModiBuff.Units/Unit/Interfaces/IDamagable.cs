namespace ModiBuff.Core.Units
{
	public interface IDamagable<out THealth, out TMaxHealth>
	{
		THealth Health { get; }
		TMaxHealth MaxHealth { get; }
	}

	public interface IAttackable<in TDamage, out TReturnDamageInfo>
	{
		TReturnDamageInfo TakeDamage(TDamage damage, IUnit source);
	}

	public interface IDamagable<out THealth, out TMaxHealth, in TDamage, out TReturnDamageInfo> :
		IDamagable<THealth, TMaxHealth>, IAttackable<TDamage, TReturnDamageInfo>
	{
	}

	public static class DamagableExtensions
	{
		public static float PercentageHealth(this IDamagable<float, float> damagable)
		{
			return damagable.Health / damagable.MaxHealth;
		}
	}
}
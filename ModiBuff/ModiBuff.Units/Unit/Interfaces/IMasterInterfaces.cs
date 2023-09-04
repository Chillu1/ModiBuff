namespace ModiBuff.Core.Units
{
	//Grouping interfaces that combine all TStat interfaces together
	public interface IMasterHealth<THealth, out TMaxHealth, in TDamage, out TReturnDamageInfo, out TReturnHealthInfo> :
		IDamagable<THealth, TMaxHealth, TDamage, TReturnDamageInfo>, IHealable<THealth, TReturnHealthInfo>, IHealthCost<THealth>
	{
	}

	public interface IMasterDamage<out THealth, out TMaxHealth, TDamage, out TReturnDamageInfo> :
		IDamagable<THealth, TMaxHealth, TDamage, TReturnDamageInfo>, IAddDamage<TDamage>, IAttacker<TDamage, TReturnDamageInfo>
	{
	}
}
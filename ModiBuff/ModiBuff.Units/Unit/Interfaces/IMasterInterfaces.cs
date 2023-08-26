namespace ModiBuff.Core.Units
{
	//Grouping interfaces that combine all TStat interfaces together
	public interface IMasterHealth<THealth, TMaxHealth, TDamage, TReturnDamageInfo, TReturnHealthInfo> :
		IDamagable<THealth, TMaxHealth, TDamage, TReturnDamageInfo>, IHealable<THealth, TReturnHealthInfo>, IHealthCost<THealth>
	{
	}

	public interface IMasterDamage<THealth, TMaxHealth, TDamage, TReturnDamageInfo> :
		IDamagable<THealth, TMaxHealth, TDamage, TReturnDamageInfo>, IAddDamage<TDamage>, IAttacker<TDamage, TReturnDamageInfo>
	{
	}
}
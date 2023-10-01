namespace ModiBuff.Core.Units
{
	//Often updated delegates
	public delegate void HealthChangedEvent(Unit unit, float newHealth, float deltaHealth);

	//Rarely updated delegates
	public delegate void DamageChangedEvent(Unit unit, float newDamage, float deltaDamage);
}
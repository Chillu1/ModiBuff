namespace ModiBuff.Core.Units
{
	//Often updated delegates
	public delegate void HealthChangedEvent(IUnit target, IUnit source, float newHealth, float deltaHealth);

	//Rarely updated delegates
	public delegate void DamageChangedEvent(Unit unit, float newDamage, float deltaDamage);
}
namespace ModiBuff.Core.Units
{
	//Often updated delegates
	public delegate void HealthChangedEvent(IUnit target, IUnit source, float newHealth, float deltaHealth);

	public delegate void DispelEvent(IUnit target, IUnit source, TagType tag);

	//Rarely updated delegates
	public delegate void DamageChangedEvent(IUnit unit, float newDamage, float deltaDamage);
}
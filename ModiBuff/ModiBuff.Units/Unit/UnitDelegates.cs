namespace ModiBuff.Core.Units
{
	//Often updated delegates
	public delegate void HealthChangedEvent(IUnit target, IUnit source, float newHealth, float deltaHealth);

	public delegate void DispelEvent(IUnit target, IUnit source, TagType tag);

	public delegate void PoisonEvent(IUnit target, IUnit source, int poisonStacks, int totalStacks, float dealtDamage);

	//Rarely updated delegates
	public delegate void DamageChangedEvent(IUnit unit, float newDamage, float deltaDamage);
}
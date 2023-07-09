namespace ModiBuff.Core
{
	public interface IDamagable
	{
		float Health { get; }
		float MaxHealth { get; }

		float TakeDamage(float damage, IUnit source, bool triggersEvents = true);
	}
}
namespace ModifierLibraryLite.Core
{
	public interface IUnit
	{
		float TakeDamage(float damage);
		float Heal(float heal);
		float Heal(IUnit target);
	}
}
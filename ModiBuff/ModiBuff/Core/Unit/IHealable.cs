namespace ModiBuff.Core
{
	public interface IHealable : IUnit
	{
		float Heal(float heal, IUnit source, bool triggersEvents = true);
	}
}
namespace ModiBuff.Core
{
	public interface IHealable
	{
		float Heal(float heal, IUnit source, bool triggersEvents = true);
	}
}
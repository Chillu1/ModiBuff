namespace ModiBuff.Core
{
	public interface IHealer
	{
		float HealValue { get; }

		float Heal(IHealable target, bool triggersEvents = true);
	}
}
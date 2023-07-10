namespace ModiBuff.Core
{
	public interface IAttacker
	{
		float Damage { get; }

		float Attack(IUnit target, bool triggersEvents = true);
	}
}
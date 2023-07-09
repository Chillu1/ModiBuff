namespace ModiBuff.Core
{
	public interface IAttacker
	{
		float Attack(IUnit target, bool triggersEvents = true);
	}
}
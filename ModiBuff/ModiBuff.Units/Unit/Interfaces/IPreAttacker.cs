namespace ModiBuff.Core.Units
{
	public interface IPreAttacker
	{
		void PreAttack(IUnit target, bool triggersEvents = true);
	}
}
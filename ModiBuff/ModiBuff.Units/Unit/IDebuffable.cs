namespace ModiBuff.Core.Units
{
	public interface IDebuffable
	{
		void AddDebuff(DebuffType debuffType, IUnit source);
		void RemoveDebuff(DebuffType debuffType, int stacksApplied, IUnit source);
		bool ContainsDebuff(DebuffType debuffType);
	}
}
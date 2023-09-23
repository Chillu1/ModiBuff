namespace ModiBuff.Core
{
	public interface IModifierApplyCheckGenerator : IModifierGenerator
	{
		bool HasApplyChecks { get; }

		ModifierCheck CreateApplyCheck();
	}
}
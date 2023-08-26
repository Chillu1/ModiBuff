namespace ModiBuff.Core
{
	public interface IModifierApplyCheckRecipe : IModifierRecipe
	{
		bool HasApplyChecks { get; }

		ModifierCheck CreateApplyCheck();
	}
}
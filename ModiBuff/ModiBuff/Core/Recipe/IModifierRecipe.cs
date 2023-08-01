namespace ModiBuff.Core
{
	public interface IModifierRecipe
	{
		int Id { get; }
		string Name { get; }

		bool HasApplyChecks { get; }

		ModifierCheck CreateApplyCheck(); //TODO Split this into two interfaces?
		Modifier Create();

		void Finish();
	}
}
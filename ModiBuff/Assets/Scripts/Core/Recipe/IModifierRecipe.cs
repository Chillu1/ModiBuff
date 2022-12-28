namespace ModiBuff.Core
{
	public interface IModifierRecipe
	{
		int Id { get; }
		string Name { get; }

		bool HasApplyChecks { get; }

		internal ModifierCheck CreateApplyCheck(); //TODO Split this into two interfaces?
		internal Modifier Create();

		void Finish();
	}
}
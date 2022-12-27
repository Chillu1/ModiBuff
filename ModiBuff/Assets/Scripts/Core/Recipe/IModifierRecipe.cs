namespace ModiBuff.Core
{
	public interface IModifierRecipe
	{
		int Id { get; }
		string Name { get; }

		bool HasChecks { get; }

		internal ModifierCheck CreateApplyCheck();
		internal Modifier Create();

		void Finish();
	}
}
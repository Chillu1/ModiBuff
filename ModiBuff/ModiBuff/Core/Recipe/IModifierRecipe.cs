namespace ModiBuff.Core
{
	public interface IModifierRecipe
	{
		int Id { get; }
		string Name { get; }

		Modifier Create();

		void Finish();
	}
}
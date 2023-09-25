namespace ModiBuff.Extensions.Godot
{
	public interface IModifierRecipeResource
	{
		string Name { get; }
		int Id { get; }
		bool NeedsSaving { get; }

		bool Validate();
		void SetName(string name);
		void SetId(int recipeId);
		void Reset();
	}
}
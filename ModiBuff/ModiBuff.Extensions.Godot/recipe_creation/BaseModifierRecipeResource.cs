using Godot;

namespace ModiBuff.Extensions.Godot
{
	public abstract partial class BaseModifierRecipeResource : Resource, IModifierRecipeResource
	{
		private int _id;

		[Export] public string Name { get; set; }

		[Export]
		public int Id //TODO Not working currently (readonly)
		{
			get => _id;
			set => _id = value;
		}

		public bool NeedsSaving { get; private set; }

		public void SetName(string name)
		{
			NeedsSaving = true;
			Name = name;
		}

		public void SetId(int recipeId)
		{
			if (_id == recipeId)
				return;

			NeedsSaving = true;
			_id = recipeId;
		}

		public void SetChanged() => NeedsSaving = true;

		public void Reset() => NeedsSaving = false;

		public virtual bool Validate()
		{
			bool valid = true;

			if (string.IsNullOrEmpty(Name))
			{
				valid = false;
				GD.PushError("Recipe name is invalid");
			}

			return valid;
		}
	}
}